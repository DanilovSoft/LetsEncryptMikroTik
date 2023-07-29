using System.Globalization;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using Certes;
using Certes.Acme;
using Certes.Acme.Resource;
using DanilovSoft.MikroApi;
using Microsoft.Extensions.Logging;

namespace LetsEncryptMikroTik.Core;

internal sealed class Acme
{
    private readonly MikroTikConnection _connection;
    private readonly CertUpdaterOptions _options;
    private readonly ILogger _logger;
    private readonly string _domainName;
    private readonly IPAddress _thisMachineIp;
    private readonly LeUri _letsEncryptAddress;

    /// <exception cref="LetsEncryptMikroTikException"/>
    public Acme(MikroTikConnection connection, CertUpdaterOptions options, ILogger logger)
    {
        ArgumentNullException.ThrowIfNull(connection);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(logger);

        _connection = connection;

        if (string.IsNullOrEmpty(options.DomainName))
            throw new ArgumentOutOfRangeException(nameof(options), "Имя домена не может быть пустым");

        _domainName = options.DomainName;
        _thisMachineIp = options.LocalIP ?? throw new ArgumentOutOfRangeException(nameof(options), "ThisMachineIp не может быть пустым");
        _letsEncryptAddress = options.LetsEncryptAddress ?? throw new ArgumentOutOfRangeException(nameof(options), "LetsEncryptAddress не может быть пустым");

        _options = options;
        _logger = logger;
    }

    /// <summary>
    /// HTTP-01 challenge.
    /// </summary>
    /// <exception cref="LetsEncryptMikroTikException"/>
    public async Task<LetsEncryptCert> GetCertAsync(string? accountPemKey = null)
    {
        AcmeContext acme;
        IAccountContext account;
        //Uri knownServer = _letsEncryptAddress;

        if (accountPemKey != null)
        {
            // Load the saved account key
            var accountKey = KeyFactory.FromPem(accountPemKey);
            acme = new AcmeContext(_letsEncryptAddress, accountKey);
            _logger.LogInformation("Авторизация в Let's Encrypt");
            account = await acme.Account().ConfigureAwait(false);
        }
        else
        {
            acme = new AcmeContext(_letsEncryptAddress);
            _logger.LogInformation("Авторизация в Let's Encrypt");
            account = await acme.NewAccount(_options.Email, termsOfServiceAgreed: true).ConfigureAwait(false);
            // Save the account key for later use
            //string pemKey = acme.AccountKey.ToPem();
        }

        _logger.LogInformation("Заказываем новый сертификат");
        var order = await acme.NewOrder(new[] { _options.DomainName }).ConfigureAwait(false);

        // Get the token and key authorization string.
        _logger.LogInformation("Получаем способы валидации заказа");
        var authz = (await order.Authorizations().ConfigureAwait(false)).First();

        if (_options.UseAlpn)
        {
            _logger.LogInformation("Выбираем TLS-ALPN-01 способ валидации");
            var challenge = await authz.TlsAlpn().ConfigureAwait(false) ?? throw new InvalidOperationException("No TLS ALPN challenge available");
            var keyAuthString = challenge.KeyAuthz;
            _ = challenge.Token;

            await ChallengeAlpnAsync(challenge, keyAuthString).ConfigureAwait(false);
        }
        else
        {
            _logger.LogInformation("Выбираем HTTP-01 способ валидации");
            var challenge = await authz.Http().ConfigureAwait(false);
            var keyAuthz = challenge.KeyAuthz;

            await HttpChallengeAsync(challenge, keyAuthz).ConfigureAwait(false);
        }

        _logger.LogInformation("Загружаем сертификат");

        // Download the certificate once validation is done
        var privateKey = KeyFactory.NewKey(KeyAlgorithm.RS256);

        var cert = await order.Generate(new CsrInfo
        {
            CommonName = _options.DomainName,
            CountryName = "CA",
            State = "Ontario",
            Locality = "Toronto",
            Organization = "Certes",
            OrganizationUnit = "Dev",
        }, privateKey).ConfigureAwait(false);

        // Export full chain certification.
        var certPem = cert.ToPem();
        var keyPem = privateKey.ToPem();

        // Export PFX
        var pfxBuilder = cert.ToPfx(privateKey);
        var pfx = pfxBuilder.Build(friendlyName: _options.DomainName, password: "");

        //await acme.RevokeCertificate(pfx, RevocationReason.Superseded, privateKey);

        using var cert2 = new X509Certificate2(pfx);
        return new LetsEncryptCert(cert2.NotAfter, certPem, keyPem, cert2.GetCommonName(), cert2.GetSha2Thumbprint());
    }

    private async Task HttpChallengeAsync(IChallengeContext httpChallenge, string keyAuthString)
    {
        using var challenge = new HttpChallenge(_options.LocalIP, keyAuthString, _logger);
        await ChallengeAsync(challenge, httpChallenge).ConfigureAwait(false);
    }

    private async Task ChallengeAlpnAsync(IChallengeContext challengeContext, string keyAuthString)
    {
        using var alpnChallenge = new AlpnChallenge(_options.LocalIP, _domainName, keyAuthString);
        await ChallengeAsync(alpnChallenge, challengeContext).ConfigureAwait(false);
    }

    private async Task ChallengeAsync(IChallenge challenge, IChallengeContext httpChallenge)
    {
        // Запустить asp net.
        _logger.LogInformation("Запускаем веб-сервер");
        challenge.Start();

        var mtNatId = MtAddDstNatRule(dstPort: challenge.PublicPort, toPorts: challenge.ListenPort);
        var mtFilterId = MtAllowPortFilter(dstPort: challenge.ListenPort, publicPort: challenge.PublicPort);
        var mtMangleId = MtAllowMangleRule(dstPort: challenge.ListenPort);

        await Task.Delay(2000).ConfigureAwait(false); // Правило в микротике начинает работать не мгновенно.

        //Challenge status;
        try
        {
            // Ask the ACME server to validate our domain ownership.
            _logger.LogInformation("Информируем Let's Encrypt что мы готовы пройти валидацию");
            var status = await httpChallenge.Validate().ConfigureAwait(false);

            var waitWithSem = true;
            while (status.Status == ChallengeStatus.Pending)
            {
                if (waitWithSem)
                {
                    _logger.LogInformation("Ожидаем 20 сек входящий HTTP запрос");
                    var t = await Task.WhenAny(Task.Delay(20_000), challenge.Completion).ConfigureAwait(false);

                    if (t == challenge.Completion)
                    {
                        waitWithSem = false;
                        _logger.LogInformation("Успешно выполнили входящий запрос; ждём 15 сек перед запросом сертификата");
                        await Task.Delay(15_000).ConfigureAwait(false);
                    }
                    else
                    // Таймаут.
                    {
                        waitWithSem = false;
                        _logger.LogInformation("Запрос ещё не поступил; дополнительно ожидаем ещё 5 сек");
                        await Task.Delay(5000).ConfigureAwait(false);
                    }
                }
                else
                {
                    _logger.LogInformation("Заказ всё ещё в статусе Pending; делаем дополнительную паузу на 5 сек");
                    await Task.Delay(5_000).ConfigureAwait(false);
                }

                _logger.LogInformation("Запрашиваем статус нашего заказа");
                status = await httpChallenge.Resource().ConfigureAwait(false);
            }

            if (status.Status == ChallengeStatus.Valid)
            {
                _logger.LogInformation("Статус заказа: Valid; загружаем сертификат");
            }
            else
            {
                _logger.LogError("Статус заказа: {Status}", status.Status);
                throw new LetsEncryptMikroTikException($"Статус заказа: {status.Status}. Ошибка: {status.Error.Detail}");
            }
        }
        finally
        {
            // Возвращаем NAT
            ClosePort(mtFilterId);
            RemoveNatRule(mtNatId);
            RemoveMangleRule(mtMangleId);
        }
    }

    private string MtAllowPortFilter(int dstPort, int publicPort)
    {
        _logger.LogInformation("Создаём правило разрешающее соединения на {PublicPort} порт в фаерволе микротика", publicPort);

        var id = _connection.Command("/ip firewall filter add")
            .Attribute("chain", "forward")
            .Attribute("dst-address", _thisMachineIp.ToString())
            .Attribute("protocol", "tcp")
            .Attribute("dst-port", $"{dstPort}")
            //.Attribute("in-interface", _config.WanIface)
            .Attribute("action", "accept")
            .Attribute("place-before", "0")
            .Attribute("comment", "Let's Encrypt challenge")
            .Scalar<string>();

        return id;
    }

    private string MtAllowMangleRule(int dstPort)
    {
        _logger.LogInformation("Создаём правило мангла для прямой маршрутизации порта {DstPort}", dstPort);

        var id = _connection.Command("/ip firewall mangle add")
            .Attribute("chain", "prerouting")
            .Attribute("src-address", _thisMachineIp.ToString())
            .Attribute("protocol", "tcp")
            .Attribute("src-port", dstPort.ToString(CultureInfo.InvariantCulture))
            .Attribute("action", "accept")
            .Attribute("passthrough", "no")
            .Attribute("place-before", "0")
            .Attribute("comment", "Let's Encrypt challenge")
            .Scalar<string>();

        return id;
    }

    private void ClosePort(string ruleId)
    {
        _logger.LogInformation("Удаляем созданное правило фаервола");

        // Удалить правило.
        _connection
            .Command("/ip firewall filter remove")
            .Attribute(".id", ruleId)
            .Send();
    }

    /// <summary>
    /// Перенастраивает NAT для доступа извне к указанному порту.
    /// </summary>
    private string MtAddDstNatRule(int dstPort, int toPorts)
    {
        _logger.LogInformation("Создаём правило NAT в микротике");

        var ruleId = _connection.Command("/ip firewall nat add")
            .Attribute("chain", "dstnat")
            .Attribute("protocol", "tcp")
            .Attribute("dst-port", $"{dstPort}")
            //.Attribute("in-interface", _config.WanIface)
            .Attribute("action", "netmap")
            .Attribute("to-addresses", _thisMachineIp.ToString())
            .Attribute("to-ports", $"{toPorts}")
            .Attribute("place-before", "0")
            .Attribute("comment", "Let's Encrypt challenge")
            .Scalar<string>();

        return ruleId;
    }

    private void RemoveNatRule(string ruleId)
    {
        _logger.LogInformation("Удаляем созданное правило NAT");

        // Удалить правило.
        _connection
            .Command("/ip firewall nat remove")
            .Attribute(".id", ruleId)
            .Send();
    }

    private void RemoveMangleRule(string ruleId)
    {
        _logger.LogInformation("Удаляем созданное правило мангла");

        // Удалить правило.
        _connection
            .Command("/ip firewall mangle remove")
            .Attribute(".id", ruleId)
            .Send();
    }
}
