using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Text;
using DanilovSoft.MikroApi;
using Microsoft.Extensions.Logging;

namespace LetsEncryptMikroTik.Core;

internal sealed class InnerCertUpdater
{
    private const string FolderName = "Cert";
    private readonly MikroTikConnection _connection;
    private readonly CertUpdaterOptions _options;
    private readonly ILogger _logger;

    public InnerCertUpdater(MikroTikConnection connection, CertUpdaterOptions options, ILogger logger)
    {
        ArgumentNullException.ThrowIfNull(connection);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(logger);
        Debug.Assert(connection.Connected);

        _connection = connection;
        _options = options;
        _logger = logger;
    }

    /// <exception cref="LetsEncryptMikroTikException"/>
    public async Task UpdateCertificateAsync(CancellationToken cancellationToken = default)
    {
        // Валидация имени WAN-интерфейса.
        CheckWanInterface(cancellationToken);

        var mtHasOldCert = TryGetExpiredAfter(out var expires, out var existedCertes, cancellationToken);
        if (mtHasOldCert)
        {
            var daysLeft = (int)expires.TotalDays;
            if (daysLeft > _options.ReplaceCertOnDaysLessThan && !_options.Force)
            {
                _logger.LogInformation("В микротике есть ещё актуальный сертификат который истекает через {DaysLeft} {DaysLeft}. Завершаем работу.", daysLeft, Days(daysLeft));
                return;
            }
            
            _logger.LogInformation("В микротике уже есть сертификат с таким Common-Name который истекает через {DaysLeft} {DaysLeft}.", daysLeft, Days(daysLeft));
        }

        LetsEncryptCert newCert;
        var certFileName = $"{_options.DomainName}-cert.pem";
        var certPrivKeyFileName = $"{_options.DomainName}-key.pem";

        if (_options.SaveFile)
        {
            var certFilePath = GetFilePath(certFileName);
            var keyFilePath = GetFilePath(certPrivKeyFileName);

            CreateDirectory(certFilePath);
            CreateDirectory(keyFilePath);

            try
            {
                // Перед загрузкой сертификата убедимся что мы сможем его сохранить.
                _logger.LogInformation("Создаём файлы в папке '{FolderName}'", FolderName);
                using (var certStream = File.CreateText(certFilePath))
                using (var keyStream = File.CreateText(keyFilePath))
                {
                    var acme = new Acme(_connection, _options, _logger);
                    newCert = await acme.GetCertAsync().ConfigureAwait(false); // Загрузить сертификат от Let's Encrypt.
                    certStream.Write(newCert.CertPem);
                    keyStream.Write(newCert.KeyPem);
                }
            }
            catch
            {
                _logger.LogInformation("Удаляем файлы в следствии ошибки");

                if (File.Exists(certFilePath))
                    File.Delete(certFilePath);

                if (File.Exists(keyFilePath))
                    File.Delete(keyFilePath);

                throw;
            }
        }
        else
        {
            var acme = new Acme(_connection, _options, _logger);
            newCert = await acme.GetCertAsync().ConfigureAwait(false); // Загрузить сертификат от Let's Encrypt.
        }

        // Загружаем сертификат и приватный ключ в микротик по FTP.
        await UploadFtpAsync(certFileName, certPrivKeyFileName, newCert, cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("Импортируем сертификат в микротик из файла '{CertFileName}'", certFileName);
        if (!TryImport(certFileName, cancellationToken))
        {
            _logger.LogError("Не удалось импортировать сертификат в микротик");
            throw new LetsEncryptMikroTikException("Не удалось импортировать сертификат в микротик.");
        }

        _logger.LogInformation("Импортируем закрытый ключ в микротик из файла '{CertPrivKeyFileName}'", certPrivKeyFileName);
        if (!TryImport(certPrivKeyFileName, cancellationToken))
        {
            _logger.LogError("Не удалось импортировать закрытый ключ в микротик");
            throw new LetsEncryptMikroTikException("Не удалось импортировать закрытый ключ в микротик.");
        }

        if (mtHasOldCert && expires.TotalDays > 1)
        {
            var daysValid = (int)expires.TotalDays;

            // Оставляем сообщение в логах микротика.
            AddWarning($"Был добавлен новый сертификат '{_options.DomainName}'. Старый сертификат будет актуален ещё {daysValid} {Days(daysValid)}");
        }
        else
        {
            // Оставляем сообщение в логах микротика.
            AddWarning($"Был добавлен новый сертификат '{_options.DomainName}'");
        }

        // Удаляем файлы из микротика.
        RemoveFile(certFileName);
        RemoveFile(certPrivKeyFileName);

        // Переименовать старый сертификат.
        RenameOldCert(existedCertes, newCert, cancellationToken);

        _logger.LogInformation("Сертификат успешно установлен");
    }

    private void CheckWanInterface(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Проверяем что в микротике есть интерфейс '{WanIface}'", _options.WanIface);

        var ifaceId = _connection.Command("/interface print")
            .Query("name", _options.WanIface)
            .Proplist(".id")
            .ScalarOrDefault<string>(cancellationToken);

        if (ifaceId == null)
        {
            throw new LetsEncryptMikroTikException($"В Микротике не найден интерфейс '{_options.WanIface}'");
        }
    }

    private bool TryGetExpiredAfter(out TimeSpan expires, out CertificateDto[] certes, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Запрашиваем у микротика информацию о сертификате с Common-Name: '{CommonName}'", _options.DomainName);

        // Может быть несколько сертификатор с одинаковым common-name.
        certes = _connection.Command("/certificate print")
            .Query("common-name", _options.DomainName)
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

    private async Task UploadFtpAsync(string certFileName, string certPrivKeyFileName, LetsEncryptCert cert, CancellationToken cancellationToken)
    {
        // Включить FTP в микротике.
        EnableFtp(out var ftpPort, out var enabledChanged, out var allowedChanged, out var allowedAddresses);
        try
        {
            // Загружаем сертификат в микротик по FTP.
            await UploadFileAsync(ftpPort, cert.CertPem, certFileName, cancellationToken).ConfigureAwait(false);

            // Загружаем закрытый ключ сертификата в микротик по FTP.
            await UploadFileAsync(ftpPort, cert.KeyPem, certPrivKeyFileName, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            RestoreFtp(enabledChanged, allowedChanged, allowedAddresses);
        }
    }

    private bool TryImport(string fileName, CancellationToken cancellationToken)
    {
        var filesImported = _connection.Command("/certificate import")
            .Attribute("file-name", fileName)
            .Attribute("passphrase", "")
            .Scalar<int>("files-imported", cancellationToken);

        return filesImported > 0;
    }

    private void RemoveFile(string fileName)
    {
        _logger.LogInformation("Удаляем файл '{FileName}' из микротика", fileName);

        _connection.Command("/file remove")
            .Attribute("numbers", fileName)
            .Send();
    }

    private void AddWarning(string message)
    {
        _logger.LogInformation("Оставляем сообщение в логах микротика");

        _connection.Command("/log warning")
            .Attribute("message", message)
            .Send();
    }

    private void EnableFtp(out int ftpPort, out bool enabledChanged, out bool allowedChanged, out string allowedAddresses)
    {
        var ftpService = _connection.Command("/ip service print")
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
                return IsInRange(_options.LocalIP, ipAddress, netMask);
            });
        }

        if (ftpService.Disabled || !connectionAllowed)
        {
            if (ftpService.Disabled)
            {
                enabledChanged = true;
                _logger.LogWarning("В микротике выключен FTP. Временно включаем его");
            }
            else
                enabledChanged = false;

            var address = ftpService.Address;
            if (!connectionAllowed)
            {
                allowedChanged = true;
                _logger.LogWarning("В микротике доступ к FTP с адреса {ThisMachineIp} не разрешён. Временно разрешаем", _options.LocalIP);
                address += $",{_options.LocalIP}/32";
            }
            else
                allowedChanged = true;

            _connection.Command("/ip service set")
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

    private void RestoreFtp(bool enabledChanged, bool allowedChanged, string allowedAddresses)
    {
        if (enabledChanged || allowedChanged)
        {
            var com = _connection.Command("/ip service set")
            .Attribute("numbers", "ftp");

            if (enabledChanged)
            {
                _logger.LogInformation("Выключаем FTP в микротике");
                com.Attribute("disabled", "true");
            }

            if (allowedChanged)
            {
                _logger.LogInformation("Убираем IP {ThisMachineIp} из разрешённых для FTP в микротике", _options.LocalIP);
                com.Attribute("address", allowedAddresses);
            }
            com.Send();
        }
    }

    private async Task UploadFileAsync(int ftpPort, string fileContent, string fileName, CancellationToken cancellationToken)
    {
#pragma warning disable SYSLIB0014 // Тип или член устарел
        var request = (FtpWebRequest)WebRequest.Create(new Uri($"ftp://{_options.MikroTikAddress}:{ftpPort}/{fileName}"));
#pragma warning restore SYSLIB0014 // Тип или член устарел
        request.Method = WebRequestMethods.Ftp.UploadFile; // Перезапишет существующий.
        request.Proxy = null;
        request.UseBinary = true;
        request.EnableSsl = false;
        request.UsePassive = true;
        request.Credentials = new NetworkCredential(_options.FtpLogin, _options.FtpPassword);

        _logger.LogInformation("Отправляем файл '{FileName}' в микротик по FTP с заменой файла если такой существует", fileName);

        using (var fileStream = new MemoryStream(Encoding.ASCII.GetBytes(fileContent)))
        {
            Stream stream;
            try
            {
                stream = request.GetRequestStream();
            }
            catch (WebException ex)
            {
                if (ex.Response is FtpWebResponse response)
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

        _logger.LogInformation("Проверяем что файл появился в микротике");

        var fileId = MtGetFileId(fileName);

        // Файл может появиться не сразу.
        if (fileId == null)
        {
            _logger.LogInformation("Файл в микротике ещё не появился; делаем паузу на 1 сек");

            await Task.Delay(1000, cancellationToken).ConfigureAwait(false);

            _logger.LogInformation("Ещё раз проверяем файл в микротике");

            fileId = MtGetFileId(fileName);
            if (fileId == null)
            {
                _logger.LogError("Файл в микротике не появился. Прекращаем попытки");
                throw new LetsEncryptMikroTikException("Не удалось загрузить файл через FTP.");
            }
        }
    }

    private string? MtGetFileId(string fileName)
    {
        var fileId = _connection.Command("/file print")
                .Query("name", fileName)
                .Proplist(".id")
                .ScalarOrDefault();

        return fileId;
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

    private void RenameOldCert(CertificateDto[] cert, LetsEncryptCert newCert, CancellationToken ct)
    {
        // Может быть несколько сертификатор с одинаковым common-name.
        var newCertes = _connection.Command("/certificate print")
            .Query("fingerprint", newCert.Thumbprint)
            .Proplist(".id,name,invalid-after")
            .ToArray<CertificateDto>(ct);

        foreach (var mtNewCert in newCertes)
        {
            var newName = "new_" + mtNewCert.Name;

            while (CertExists(newName, ct))
            {
                newName = "new_" + newName;
            }

            _connection.Command("/certificate set")
                .Attribute("numbers", mtNewCert.Id)
                .Attribute("name", newName)
                .Send(ct);
        }

        RenameOldMtCerts(cert, ct);
    }

    private void RenameOldMtCerts(CertificateDto[] cert, CancellationToken ct)
    {
        foreach (var c in cert)
        {
            if (c.Name!.StartsWith("old_", StringComparison.Ordinal))
            {
                continue;
            }

            var newOldName = "old_" + c.Name;

            while (CertExists(newOldName, ct))
            {
                newOldName = "old_" + newOldName;
            }

            _connection.Command("/certificate set")
                .Attribute("numbers", c.Id)
                .Attribute("name", newOldName)
                .Send(ct);
        }
    }

    private bool CertExists(string mtName, CancellationToken cancellationToken)
    {
        var newCertes = _connection.Command("/certificate print")
               .Query("name", mtName)
               .Proplist(".id,name")
               .ToArray<CertificateDto>(cancellationToken);

        return newCertes.Length > 0;
    }
}
