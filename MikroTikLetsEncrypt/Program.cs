using DanilovSoft.MikroApi;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LetsEncryptMikroTik.Core;

public sealed class Program
{
    private readonly string _mikrotikAddress;
    private readonly ILogger _logger;

    public Program(Options config, ILogger logger)
    {
        ArgumentNullException.ThrowIfNull(config);
        ArgumentNullException.ThrowIfNull(logger);

        Config = config;
        _logger = logger;
        _mikrotikAddress = config.MikroTikAddress ?? throw new ArgumentOutOfRangeException(nameof(config), "Должен быть задан адрес микротика");
    }

    internal Options Config { get; }

    public static Tuple<string, Uri>[] GetAddresses()
    {
        return new Tuple<string, Uri>[2]
        {
            new("StagingV2", Certes.Acme.WellKnownServers.LetsEncryptStagingV2),
            new("LetsEncryptV2", Certes.Acme.WellKnownServers.LetsEncryptV2)
        };
    }

    /// <exception cref="LetsEncryptMikroTikException"/>
    public async Task RunAsync(bool logToFile = true, InMemorySink? logSink = null, CancellationToken cancellationToken = default)
    {
        CreateLogger(logSink, logToFile);

        Config.ThisMachineIp = await GetLocalEndPointForAsync(_mikrotikAddress, Config.MikroTikPort, cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("Определён IP адрес текущей машины: {ThisMachineIp}", Config.ThisMachineIp);

        try
        {
            await MainAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (LetsEncryptMikroTikException ex)
        {
            _logger.Fatal(ex.Message, "Критическая ошибка.");
            throw;
        }
        catch (Exception ex)
        {
            _logger.Fatal(ex, "Критическая ошибка.");
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

    private async Task MainAsync(CancellationToken cancellationToken)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        _logger.LogInformation("Подключение к микротику");

        var connection = new MikroTikConnection(Encoding.GetEncoding("windows-1251"));

        try
        {
            try
            {
                connection.Connect(Config.MikroTikLogin, Config.MikroTikPassword ?? "", Config.MikroTikAddress, useSsl: false, Config.MikroTikPort, cancellationToken);
            }
            catch (MikroApiTrapException ex) when (ex.Message == "std failure: not allowed (9)")
            {
                _logger.LogError("Не удалось авторизоваться пользователю '{MikroTikLogin}'. Проверьте права пользователя на доступ к api", Config.MikroTikLogin);
                throw new LetsEncryptMikroTikException($"Не удалось авторизоваться пользователю '{Config.MikroTikLogin}'. Проверьте права пользователя на доступ к api.", ex);
            }
            catch (MikroApiException ex)
            {
                throw new LetsEncryptMikroTikException("Не удалось подключиться к микротику", ex);
            }

            var activeConnection = new CertUpdater(connection);

            try
            {
                await activeConnection.UpdateCertificateAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (MikroApiTrapException ex) when (ex.Message == "not enough permissions (9)")
            {
                _logger.LogError("У пользователя '{MikroTikLogin}' недостаточно прав в микротике; требуются политики: api, read, write, ftp", Config.MikroTikLogin);
                throw new LetsEncryptMikroTikException($"У пользователя '{Config.MikroTikLogin}' недостаточно прав в микротике. Требуются права: api, read, write, ftp.", ex);
            }
        }
        finally
        {
            _logger.LogInformation("Отключение от микротика");
            await connection.DisposeAsync().ConfigureAwait(false);
        }
    }
}
