﻿using DanilovSoft.MikroApi;
using Serilog;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LetsEncryptMikroTik.Core;

public sealed class Program
{
    private const string FolderName = "Cert";
    private readonly string mikrotikAddress;
    internal ConfigClass Config { get; }

    public Program(ConfigClass config)
    {
        Config = config ?? throw new ArgumentNullException(nameof(config));

        mikrotikAddress = config.MikroTikAddress ?? throw new ArgumentOutOfRangeException(nameof(config), "Должен быть задан адрес микротика");
    }

    public static Tuple<string, Uri>[] GetAddresses()
    {
        return new Tuple<string, Uri>[2]
        {
            new Tuple<string, Uri>("StagingV2", Certes.Acme.WellKnownServers.LetsEncryptStagingV2),
            new Tuple<string, Uri>("LetsEncryptV2", Certes.Acme.WellKnownServers.LetsEncryptV2)
        };
    }

    private static void CreateLogger(InMemorySink? logSink, bool writeToFile)
    {
        var loggerBuilder = new LoggerConfiguration();

        if (writeToFile)
        {
            loggerBuilder.WriteTo.File("program.log");
        }

        if (logSink != null)
        {
            loggerBuilder
                .WriteTo.Sink(logSink);
        }

        if (Environment.UserInteractive)
        {
            loggerBuilder
                .WriteTo.Console();
        }

        Log.Logger = loggerBuilder.CreateLogger();
    }

    public async Task RunAsync(bool logToFile = true, InMemorySink? logSink = null, CancellationToken cancellationToken = default)
    {
        CreateLogger(logSink, logToFile);

        Config.ThisMachineIp = await GetLocalEndPointForAsync(mikrotikAddress, Config.MikroTikPort, cancellationToken).ConfigureAwait(false);

        Log.Information("Определён IP адрес текущей машины: {ThisMachineIp}", Config.ThisMachineIp);

        try
        {
            await MainAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (LetsEncryptMikroTikException ex)
        {
            Log.Fatal(ex.Message, "Критическая ошибка.");
            throw;
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Критическая ошибка.");
            throw;
        }
        finally
        {
            Log.CloseAndFlush();
        }

        Log.Information("Приложение успешно завершено.");
        Log.CloseAndFlush();
    }

    /// <summary>
    /// Использует интересный хак для определения по таблице маршрутизации какой адрес следует использовать
    /// для подключения к заданному адресу.
    /// </summary>
    private static async Task<IPAddress> GetLocalEndPointForAsync(string host, int port, CancellationToken cancellationToken)
    {
        if (!IPAddress.TryParse(host, out var remote))
        {
            var dns = new DnsEndPoint(host, port, AddressFamily.InterNetwork);
            Log.Information("Определяем IP-адрес для узла {Host}.", dns.Host);
            var entry = await Dns.GetHostEntryAsync(dns.Host, cancellationToken).ConfigureAwait(false);
            remote = entry.AddressList[0];
        }

        using (var sock = new Socket(remote.AddressFamily, SocketType.Dgram, ProtocolType.IP))
        {
            // Just picked a random port, you could make this application
            // specific if you want, but I don't think it really matters.
            sock.Connect(new IPEndPoint(remote, 0));

            return ((IPEndPoint)sock.LocalEndPoint!).Address;
        }
    }

    /// <exception cref="LetsEncryptMikroTikException"/>
    private async Task MainAsync(CancellationToken cancellationToken)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        Log.Information("Подключение к микротику.");

        var con = new MikroTikConnection(Encoding.GetEncoding("windows-1251"));
        try
        {
            try
            {
                con.Connect(Config.MikroTikLogin, Config.MikroTikPassword, Config.MikroTikAddress, useSsl: false, Config.MikroTikPort, cancellationToken);
            }
            catch (MikroApiTrapException ex) when (ex.Message == "std failure: not allowed (9)")
            {
                Log.Error("Не удалось авторизоваться пользователю '{MikroTikLogin}'. Проверьте права пользователя на доступ к api.", Config.MikroTikLogin);
                throw new LetsEncryptMikroTikException($"Не удалось авторизоваться пользователю '{Config.MikroTikLogin}'. Проверьте права пользователя на доступ к api.", ex);
            }
            catch (MikroApiException ex)
            {
                throw new LetsEncryptMikroTikException("Не удалось подключиться к микротику", ex);
            }

            try
            {
                await UpdateCertificateAsync(con, cancellationToken).ConfigureAwait(false);
            }
            catch (MikroApiTrapException ex) when (ex.Message == "not enough permissions (9)")
            {
                Log.Error("У пользователя '{MikroTikLogin}' недостаточно прав в микротике. Требуются права: api, read, write, ftp.", Config.MikroTikLogin);
                throw new LetsEncryptMikroTikException($"У пользователя '{Config.MikroTikLogin}' недостаточно прав в микротике. Требуются права: api, read, write, ftp.", ex);
            }
        }
        finally
        {
            Log.Information("Отключение от микротика.");
            await con.DisposeAsync().ConfigureAwait(false);
        }
    }

    private async Task UpdateCertificateAsync(MikroTikConnection con, CancellationToken ct)
    {
        // Валидация имени WAN-интерфейса.
        CheckWanInterface(con, ct);

        var mtHasOldCert = TryGetExpiredAfter(con, Config.DomainName, out var expires, out var existedCertes, ct);
        if (mtHasOldCert)
        {
            var daysLeft = (int)expires.TotalDays;
            if (daysLeft > Config.ReplaceCertOnDaysLessThan && !Config.Force)
            {
                Log.Information("В микротике есть ещё актуальный сертификат который истекает через {DaysLeft} {DaysLeft}. Завершаем работу.", daysLeft, Days(daysLeft));
                return;
            }
            else
            {
                Log.Information("В микротике уже есть сертификат с таким Common-Name который истекает через {DaysLeft} {DaysLeft}.", daysLeft, Days(daysLeft));
            }
        }

        LetsEncryptCert newCert;
        var certFileName = $"{Config.DomainName}-cert.pem";
        var certPrivKeyFileName = $"{Config.DomainName}-key.pem";

        if (Config.SaveFile)
        {
            var certFilePath = GetFilePath(certFileName);
            var keyFilePath = GetFilePath(certPrivKeyFileName);

            CreateDirectory(certFilePath);
            CreateDirectory(keyFilePath);

            try
            {
                // Перед загрузкой сертификата убедимся что мы сможем его сохранить.
                Log.Information("Создаём файлы в папке '{FolderName}'.", FolderName);
                using (var certStream = File.CreateText(certFilePath))
                using (var keyStream = File.CreateText(keyFilePath))
                {
                    var acme = new Acme(con, Config);
                    newCert = await acme.GetCertAsync().ConfigureAwait(false); // Загрузить сертификат от Let's Encrypt.
                    certStream.Write(newCert.CertPem);
                    keyStream.Write(newCert.KeyPem);
                }
            }
            catch
            {
                Log.Information("Удаляем файлы в следствии ошибки.");

                if (File.Exists(certFilePath))
                    File.Delete(certFilePath);

                if (File.Exists(keyFilePath))
                    File.Delete(keyFilePath);

                throw;
            }
        }
        else
        {
            var acme = new Acme(con, Config);

            // Загрузить сертификат от Let's Encrypt.
            newCert = await acme.GetCertAsync().ConfigureAwait(false);
        }
        
        // Загружаем сертификат и приватный ключ в микротик по FTP.
        await UploadFtpAsync(con, certFileName, certPrivKeyFileName, newCert, ct).ConfigureAwait(false);

        Log.Information("Импортируем сертификат в микротик из файла '{CertFileName}'", certFileName);
        if (!TryImport(con, certFileName, ct))
        {
            Log.Error("Не удалось импортировать сертификат в микротик.");
            throw new LetsEncryptMikroTikException("Не удалось импортировать сертификат в микротик.");
        }

        Log.Information("Импортируем закрытый ключ в микротик из файла '{CertPrivKeyFileName}'", certPrivKeyFileName);
        if (!TryImport(con, certPrivKeyFileName, ct))
        {
            Log.Error("Не удалось импортировать закрытый ключ в микротик.");
            throw new LetsEncryptMikroTikException("Не удалось импортировать закрытый ключ в микротик.");
        }

        if (mtHasOldCert && expires.TotalDays > 1)
        {
            var daysValid = (int)expires.TotalDays;

            // Оставляем сообщение в логах микротика.
            AddWarning(con, $"Был добавлен новый сертификат '{Config.DomainName}'. Старый сертификат будет актуален ещё {daysValid} {Days(daysValid)}");
        }
        else
        {
            // Оставляем сообщение в логах микротика.
            AddWarning(con, $"Был добавлен новый сертификат '{Config.DomainName}'");
        }

        // Удаляем файлы из микротика.
        RemoveFile(con, certFileName);
        RemoveFile(con, certPrivKeyFileName);

        // Переименовать старый сертификат.
        RenameOldCert(con, existedCertes, newCert, ct);

        Log.Information("Сертификат успешно установлен.");
    }

    private static void RenameOldCert(MikroTikConnection con, CertificateDto[] cert, LetsEncryptCert newCert, CancellationToken ct)
    {
        // Может быть несколько сертификатор с одинаковым common-name.
        var newCertes = con.Command("/certificate print")
            .Query("fingerprint", newCert.Thumbprint)
            .Proplist(".id,name,invalid-after")
            .ToArray<CertificateDto>(ct);

        foreach (var mtNewCert in newCertes)
        {
            var newName = "new_" + mtNewCert.Name;

            while (CertExists(con, newName, ct))
            {
                newName = "new_" + newName;
            }

            con.Command("/certificate set")
                .Attribute("numbers", mtNewCert.Id)
                .Attribute("name", newName)
                .Send(ct);
        }

        RenameOldMtCerts(con, cert, ct);
    }

    private static void RenameOldMtCerts(MikroTikConnection con, CertificateDto[] cert, CancellationToken ct)
    {
        foreach (var c in cert)
        {
            if (!c.Name!.StartsWith("old_", StringComparison.Ordinal))
            {
                var newOldName = "old_" + c.Name;

                while (CertExists(con, newOldName, ct))
                {
                    newOldName = "old_" + newOldName;
                }

                con.Command("/certificate set")
                    .Attribute("numbers", c.Id)
                    .Attribute("name", newOldName)
                    .Send(ct);
            }
        }
    }

    private static bool CertExists(MikroTikConnection con, string mtName, CancellationToken cancellationToken)
    {
        var newCertes = con.Command("/certificate print")
               .Query("name", mtName)
               .Proplist(".id,name")
               .ToArray<CertificateDto>(cancellationToken);

        return newCertes.Length > 0;
    }

    private void CheckWanInterface(MikroTikConnection con, CancellationToken cancellationToken)
    {
        Log.Information($"Проверяем что в микротике есть интерфейс '{Config.WanIface}'.");

        var ifaceId = con.Command("/interface print")
            .Query("name", Config.WanIface)
            .Proplist(".id")
            .ScalarOrDefault<string>(cancellationToken);

        if (ifaceId == null)
        {
            throw new LetsEncryptMikroTikException($"В Микротике не найден интерфейс '{Config.WanIface}'");
        }
    }

    private static void CreateDirectory(string filePath)
    {
        var directoryPath = Path.GetDirectoryName(filePath);
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }
    }

    private static string GetFilePath(string fileName)
    {
        foreach (var c in Path.GetInvalidFileNameChars())
        {
            fileName = fileName.Replace(c, '_');
        }

        var extension = Path.GetExtension(fileName);
        var fn = Path.GetFileNameWithoutExtension(fileName);
        string filePath;

        var n = 0;
        do
        {
            filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, FolderName, fileName);

            if (File.Exists(filePath))
            {
                n++;
                fileName = fn + $" ({n})" + extension;
            }
            else
                return filePath;

        } while (true);
    }

    private async Task UploadFtpAsync(MikroTikConnection con, string certFileName, string certPrivKeyFileName, LetsEncryptCert cert, CancellationToken cancellationToken)
    {
        // Включить FTP в микротике.
        EnableFtp(con, out var ftpPort, out var enabledChanged, out var allowedChanged, out var allowedAddresses);
        try
        {
            // Загружаем сертификат в микротик по FTP.
            await UploadFileAsync(con, ftpPort, cert.CertPem, certFileName, cancellationToken).ConfigureAwait(false);

            // Загружаем закрытый ключ сертификата в микротик по FTP.
            await UploadFileAsync(con, ftpPort, cert.KeyPem, certPrivKeyFileName, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            RestoreFtp(con, enabledChanged, allowedChanged, allowedAddresses);
        }
    }

    private static bool TryGetExpiredAfter(MikroTikConnection con, string commonName, out TimeSpan expires, out CertificateDto[] certes, CancellationToken cancellationToken)
    {
        Log.Information($"Запрашиваем у микротика информацию о сертификате с Common-Name: '{commonName}'.");

        // Может быть несколько сертификатор с одинаковым common-name.
        certes = con.Command("/certificate print")
            .Query("common-name", commonName)
            .Proplist(".id,name,invalid-after")
            .ToArray<CertificateDto>(cancellationToken);

        var invalidAfter = certes.Select(x => new { Cert = x, InvalidAfter = (DateTime?)DateTime.Parse(x.InvalidAfter, CultureInfo.InvariantCulture) })
            .DefaultIfEmpty()
            .Min(x => x?.InvalidAfter);

        if (invalidAfter != null)
        {
            expires = invalidAfter.Value - DateTime.Now;
            return true;
        }

        expires = default;
        return false;
    }

    private static bool TryImport(MikroTikConnection con, string fileName, CancellationToken cancellationToken)
    {
        var filesImported = con.Command("/certificate import")
            .Attribute("file-name", fileName)
            .Attribute("passphrase", "")
            .Scalar<int>("files-imported", cancellationToken);

        return filesImported > 0;
    }

    private static string Days(int days)
    {
        var n = Math.Abs(days) % 100;
        if ((n % 10 == 0) || (n % 10 >= 5 && n % 10 <= 9) || (n > 9 && n < 20))
        {
            return "дней";
        }
        else if (n % 10 == 1)
        {
            return "день";
        }
        else
        {
            return "дня";
        }
    }

    private async Task UploadFileAsync(MikroTikConnection con, int ftpPort, string fileContent, string fileName, CancellationToken cancellationToken)
    {
        var request = (FtpWebRequest)WebRequest.Create(new Uri($"ftp://{Config.MikroTikAddress}:{ftpPort}/{fileName}"));
        request.Method = WebRequestMethods.Ftp.UploadFile; // Перезапишет существующий.
        request.Proxy = null;
        request.UseBinary = true;
        request.EnableSsl = false;
        request.UsePassive = true;
        request.Credentials = new NetworkCredential(Config.FtpLogin, Config.FtpPassword);

        Log.Information($"Отправляем файл '{fileName}' в микротик по FTP с заменой файла если такой существует.");

        using (var fileStream = new MemoryStream(Encoding.ASCII.GetBytes(fileContent)))
        {
            Stream stream;
            try
            {
                stream = request.GetRequestStream();
            }
            catch (WebException ex)
            {
                if(ex.Response is FtpWebResponse response)
                {
                    if (response.StatusCode == FtpStatusCode.NotLoggedIn)
                        throw new WebException("Не удалось авторизоваться на FTP (не верный пароль?).", ex);
                }
                throw new WebException("Ошибка доступа к FTP микротика.", ex);  
            }

            using (stream)
            {
                fileStream.CopyTo(stream);
                fileStream.Flush();
            }
        }

        // Файл в микротике будет доступен через небольшой интервал.
        await Task.Delay(200, cancellationToken).ConfigureAwait(false);

        Log.Information("Проверяем что файл появился в микротике.");

        var fileId = MtGetFileId(con, fileName);

        // Файл может появиться не сразу.
        if (fileId == null)
        {
            Log.Information("Файл в микротике ещё не появился. Делаем паузу на 1 сек.");

            await Task.Delay(1000, cancellationToken).ConfigureAwait(false);

            Log.Information("Ещё раз проверяем файл в микротике.");

            fileId = MtGetFileId(con, fileName);
            if (fileId == null)
            {
                Log.Error("Файл в микротике не появился. Прекращаем попытки.");
                throw new LetsEncryptMikroTikException("Не удалось загрузить файл через FTP.");
            }
        }
    }

    private static string? MtGetFileId(MikroTikConnection con, string fileName)
    {
        var fileId = con.Command("/file print")
                .Query("name", fileName)
                .Proplist(".id")
                .ScalarOrDefault();

        return fileId;
    }

    private static void RemoveFile(MikroTikConnection con, string fileName)
    {
        Log.Information($"Удаляем файл '{fileName}' из микротика.");

        var res = con.Command("/file remove")
            .Attribute("numbers", fileName)
            .Send();
    }

    private static void AddWarning(MikroTikConnection con, string message)
    {
        Log.Information("Оставляем сообщение в логах микротика.");

        con.Command("/log warning")
            .Attribute("message", message)
            .Send();
    }

    private void EnableFtp(MikroTikConnection con, out int ftpPort, out bool enabledChanged, out bool allowedChanged, out string allowedAddresses)
    {
        var ftpService = con.Command("/ip service print")
            .Query("name", "ftp")
            .Proplist("disabled,port,address")
            .Single<IpService>();

        ftpPort = ftpService.Port;
        allowedAddresses = ftpService.Address;

        var connectionAllowed = true;
        if (ftpService.Addresses != null)
        {
            connectionAllowed = ftpService.Addresses.Any(x =>
            {
                if (string.IsNullOrEmpty(x))
                    return true;

                var parts = x.Split('/');
                var ipAddress = IPAddress.Parse(parts[0]);
                var netMask = int.Parse(parts[1], CultureInfo.InvariantCulture);
                return IsInRange(Config.ThisMachineIp, ipAddress, netMask);
            });
        }

        if (ftpService.Disabled || !connectionAllowed)
        {
            if (ftpService.Disabled)
            {
                enabledChanged = true;
                Log.Warning("В микротике выключен FTP. Временно включаем его.");
            }
            else
                enabledChanged = false;

            var address = ftpService.Address;
            if (!connectionAllowed)
            {
                allowedChanged = true;
                Log.Warning($"В микротике доступ к FTP с адреса {Config.ThisMachineIp} не разрешён. Временно разрешаем.");
                address += $",{Config.ThisMachineIp}/32";
            }
            else
                allowedChanged = true;

            con.Command("/ip service set")
                .Attribute("numbers", "ftp")
                .Attribute("disabled", "false")
                .Attribute("address", address)
                .Send();
        }
        else
        {
            allowedChanged = false;
            enabledChanged = false;
        }
    }

    // true if ipAddress falls inside the CIDR range, example
    // bool result = IsInRange("10.50.30.7", "10.0.0.0/8");
    private static bool IsInRange(IPAddress ipAddress, IPAddress ipAddress2, int netMask)
    {
        var mask = BitsToIpAddress(netMask);
        return ipAddress.IsInSameSubnet(ipAddress2, mask);
    }

    private void RestoreFtp(MikroTikConnection con, bool enabledChanged, bool allowedChanged, string allowedAddresses)
    {
        if (enabledChanged || allowedChanged)
        {
            var com = con.Command("/ip service set")
            .Attribute("numbers", "ftp");

            if (enabledChanged)
            {
                Log.Information("Выключаем FTP в микротике.");
                com.Attribute("disabled", "true");
            }

            if (allowedChanged)
            {
                Log.Information($"Убираем IP {Config.ThisMachineIp} из разрешённых для FTP в микротике.");
                com.Attribute("address", allowedAddresses);
            }
            com.Send();
        }
    }

    private static IPAddress BitsToIpAddress(int bits)
    {
        if (bits < 0 || bits > 32)
            throw new ArgumentOutOfRangeException(nameof(bits));

        if (bits == 32)
            return new IPAddress(uint.MaxValue);

        var mask = ~(uint.MaxValue >> bits);
        //uint mask = (uint.MaxValue << (32 - bits));
        var bytes = BitConverter.GetBytes(mask);
        Array.Reverse(bytes);
        return new IPAddress(bytes);
    }
}
