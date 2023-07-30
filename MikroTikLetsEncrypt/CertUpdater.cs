using DanilovSoft.MikroApi;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace LetsEncryptMikroTik.Core;

public sealed class CertUpdater
{
    private readonly Options _options;
    private readonly ILogger _logger;

    public CertUpdater(Options options, ILogger logger)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(logger);

        _options = options;
        _logger = logger;
    }

    public static Tuple<string, Uri>[] GetAddresses()
    {
        return new Tuple<string, Uri>[2]
        {
            new("StagingV2", Certes.Acme.WellKnownServers.LetsEncryptStagingV2),
            new("LetsEncryptV2", Certes.Acme.WellKnownServers.LetsEncryptV2)
        };
    }

    /// <exception cref="LetsEncryptMikroTikException"/>
    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await RunCore(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Критическая ошибка");
            throw;
        }

        _logger.LogInformation("Приложение успешно завершено");
    }

    /// <summary>
    /// Использует интересный хак для определения по таблице маршрутизации какой адрес следует использовать
    /// для подключения к заданному адресу.
    /// </summary>
    private async Task<IPAddress> GetLocalIP(string host, int port, CancellationToken cancellationToken)
    {
        if (!IPAddress.TryParse(host, out var hostIPAddress)) // Если хост не является айпишником то резолвим IP.
        {
            var dns = new DnsEndPoint(host, port, AddressFamily.InterNetwork);
            _logger.LogInformation("Определяем IP-адрес для узла {Host}.", dns.Host);
            var entry = await Dns.GetHostEntryAsync(dns.Host, cancellationToken).ConfigureAwait(false);
            hostIPAddress = entry.AddressList[0];
        }

        return GetLocalIP(hostIPAddress);
    }

    private static IPAddress GetLocalIP(IPAddress hostAddress)
    {
        using var socket = new Socket(hostAddress.AddressFamily, SocketType.Dgram, ProtocolType.IP);
        socket.Connect(new IPEndPoint(hostAddress, 0));
        return ((IPEndPoint)socket.LocalEndPoint!).Address;
    }

    private async Task RunCore(CancellationToken cancellationToken)
    {
        var localIP = await GetLocalIP(_options.MikroTikAddress, _options.MikroTikPort, cancellationToken).ConfigureAwait(false);
        _logger.LogInformation("Определён IP адрес текущей машины: {ThisMachineIp}", localIP);

        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        _logger.LogInformation("Подключение к микротику");

        var connection = new MikroTikConnection(Encoding.GetEncoding("windows-1251"));

        try
        {
            try
            {
                connection.Connect(_options.MikroTikLogin, _options.MikroTikPassword ?? "", _options.MikroTikAddress, useSsl: false, _options.MikroTikPort, cancellationToken);
            }
            catch (MikroApiTrapException ex) when (ex.Message == "std failure: not allowed (9)")
            {
                _logger.LogError("Не удалось авторизоваться пользователю '{MikroTikLogin}'. Проверьте права пользователя на доступ к api", _options.MikroTikLogin);
                throw new LetsEncryptMikroTikException($"Не удалось авторизоваться пользователю '{_options.MikroTikLogin}'. Проверьте права пользователя на доступ к api.", ex);
            }
            catch (MikroApiException ex)
            {
                throw new LetsEncryptMikroTikException("Не удалось подключиться к микротику", ex);
            }

            var options = new CertUpdaterOptions
            {
                MikroTikAddress = _options.MikroTikAddress,
                LocalIP = localIP,
                WanIface = _options.WanIface,
                LetsEncryptAddress = _options.LetsEncryptAddress,
                DomainName = _options.DomainName,
                Email = _options.Email,
                Force = _options.Force,
                FtpLogin = _options.FtpLogin,
                FtpPassword = _options.FtpPassword,
                VerificationMethod = _options.VerificationMethod,
                ReplaceCertOnDaysLessThan = _options.ReplaceCertOnDaysLessThan,
                SaveFile = _options.SaveFile
            };
            var activeConnection = new InnerCertUpdater(connection, options, _logger);

            try
            {
                await activeConnection.UpdateCertificateAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (MikroApiTrapException ex) when (ex.Message == "not enough permissions (9)")
            {
                _logger.LogError("У пользователя '{MikroTikLogin}' недостаточно прав в микротике; требуются политики: api, read, write, ftp", _options.MikroTikLogin);
                throw new LetsEncryptMikroTikException($"У пользователя '{_options.MikroTikLogin}' недостаточно прав в микротике. Требуются права: api, read, write, ftp.", ex);
            }
        }
        finally
        {
            _logger.LogInformation("Отключение от микротика");
            await connection.DisposeAsync().ConfigureAwait(false);
        }
    }
}
