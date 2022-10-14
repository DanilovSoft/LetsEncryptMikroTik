using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Certes;
using Certes.Acme;
using Certes.Acme.Resource;
using DanilovSoft.MikroApi;
using Serilog;

namespace LetsEncryptMikroTik.Core
{
    internal sealed class Acme
    {
        //private static readonly Uri KnownServer = WellKnownServers.LetsEncryptStagingV2;
        private readonly MikroTikConnection _mtCon;
        private readonly ConfigClass _config;
        private readonly string _domainName;
        private readonly IPAddress _thisMachineIp;
        private readonly LeUri _letsEncryptAddress;

        public Acme(MikroTikConnection connection, ConfigClass config)
        {
            _mtCon = connection;

            if (string.IsNullOrEmpty(config.DomainName))
                throw new ArgumentOutOfRangeException(nameof(config), "Имя домена не может быть пустым");

            _domainName = config.DomainName;
            _thisMachineIp = config.ThisMachineIp ?? throw new ArgumentOutOfRangeException(nameof(config), "ThisMachineIp не может быть пустым");
            _letsEncryptAddress = config.LetsEncryptAddress ?? throw new ArgumentOutOfRangeException(nameof(config), "LetsEncryptAddress не может быть пустым");

            _config = config;
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
                Log.Information("Авторизация в Let's Encrypt.");
                account = await acme.Account().ConfigureAwait(false);
            }
            else
            {
                acme = new AcmeContext(_letsEncryptAddress);
                Log.Information("Авторизация в Let's Encrypt.");
                account = await acme.NewAccount(_config.Email, termsOfServiceAgreed: true).ConfigureAwait(false);
                // Save the account key for later use
                //string pemKey = acme.AccountKey.ToPem();
            }

            Log.Information("Заказываем новый сертификат.");
            var order = await acme.NewOrder(new[] { _config.DomainName }).ConfigureAwait(false);

            // Get the token and key authorization string.
            Log.Information("Получаем способы валидации заказа.");
            var authz = (await order.Authorizations().ConfigureAwait(false)).First();

            if (_config.UseAlpn)
            {
                Log.Information("Выбираем TLS-ALPN-01 способ валидации.");
                var challenge = await authz.TlsAlpn().ConfigureAwait(false);
                if (challenge is null)
                {
                    throw new InvalidOperationException("No TLS ALPN challenge available");
                }

                var keyAuthz = challenge.KeyAuthz;
                var token = challenge.Token;

                await ChallengeAlpnAsync(challenge, keyAuthz).ConfigureAwait(false);
            }
            else
            {
                Log.Information("Выбираем HTTP-01 способ валидации.");
                var challenge = await authz.Http().ConfigureAwait(false);
                var keyAuthz = challenge.KeyAuthz;

                await HttpChallengeAsync(challenge, keyAuthz).ConfigureAwait(false);
            }

            Log.Information("Загружаем сертификат.");

            // Download the certificate once validation is done
            var privateKey = KeyFactory.NewKey(KeyAlgorithm.RS256);

            var cert = await order.Generate(new CsrInfo
            {
                CommonName = _config.DomainName,
                CountryName = "CA",
                State = "Ontario",
                Locality = "Toronto",
                Organization = "Certes",
                OrganizationUnit = "Dev",
            }, privateKey)
                .ConfigureAwait(false);

            // Export full chain certification.
            var certPem = cert.ToPem();
            var keyPem = privateKey.ToPem();

            // Export PFX
            var pfxBuilder = cert.ToPfx(privateKey);
            var pfx = pfxBuilder.Build(friendlyName: _config.DomainName, password: "");

            //await acme.RevokeCertificate(pfx, RevocationReason.Superseded, privateKey);

            using (var cert2 = new X509Certificate2(pfx))
            {
                return new LetsEncryptCert(cert2.NotAfter, certPem, keyPem, cert2.GetCommonName(), cert2.GetSha2Thumbprint());
            }
        }

        /// <exception cref="LetsEncryptMikroTikException"/>
        private async Task HttpChallengeAsync(IChallengeContext httpChallenge, string keyAuthString)
        {
            using (var challenge = new HttpChallenge(_config, keyAuthString))
            {
                await ChallengeAsync(challenge, httpChallenge).ConfigureAwait(false);
            }
        }

        /// <exception cref="LetsEncryptMikroTikException"/>
        private async Task ChallengeAlpnAsync(IChallengeContext challengeContext, string keyAuthString)
        {
            using (var alpnChallenge = new AlpnChallenge(_config, _domainName, keyAuthString))
            {
                await ChallengeAsync(alpnChallenge, challengeContext).ConfigureAwait(false);
            }
        }

        /// <exception cref="LetsEncryptMikroTikException"/>
        private async Task ChallengeAsync(IChallenge challenge, IChallengeContext httpChallenge)
        {
            // Запустить asp net.
            Log.Information("Запускаем веб-сервер.");
            challenge.Start();

            var mtNatId = MtAddDstNatRule(dstPort: challenge.PublicPort, toPorts: challenge.ListenPort);
            var mtFilterId = MtAllowPortFilter(dstPort: challenge.ListenPort, publicPort: challenge.PublicPort);
            var mtMangleId = MtAllowMangleRule(dstPort: challenge.ListenPort);

            // Правило в микротике начинает работать не мгновенно.
            await Task.Delay(2000).ConfigureAwait(false);

            //Challenge status;
            try
            {
                // Ask the ACME server to validate our domain ownership.
                Log.Information("Информируем Let's Encrypt что мы готовы пройти валидацию.");
                var status = await httpChallenge.Validate().ConfigureAwait(false);

                var waitWithSem = true;
                while (status.Status == ChallengeStatus.Pending)
                {
                    if (waitWithSem)
                    {
                        Log.Information("Ожидаем 20 сек. входящий HTTP запрос.");
                        var t = await Task.WhenAny(Task.Delay(20_000), challenge.Completion).ConfigureAwait(false);

                        if (t == challenge.Completion)
                        {
                            waitWithSem = false;
                            Log.Information("Успешно выполнили входящий запрос. Ждём 15 сек. перед запросом сертификата.");
                            await Task.Delay(15_000).ConfigureAwait(false);
                        }
                        else
                        // Таймаут.
                        {
                            waitWithSem = false;
                            Log.Information("Запрос ещё не поступил. Дополнительно ожидаем ещё 5 сек.");
                            await Task.Delay(5000).ConfigureAwait(false);
                        }
                    }
                    else
                    {
                        Log.Information("Заказ всё ещё в статусе Pending. Делаем дополнительную паузу на 5 сек.");
                        await Task.Delay(5_000).ConfigureAwait(false);
                    }

                    Log.Information("Запрашиваем статус нашего заказа.");
                    status = await httpChallenge.Resource().ConfigureAwait(false);
                }

                if (status.Status == ChallengeStatus.Valid)
                {
                    Log.Information("Статус заказа: Valid. Загружаем сертификат.");
                }
                else
                {
                    Log.Error($"Статус заказа: {status.Status}");
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
            Log.Information($"Создаём правило разрешающее соединения на {publicPort} порт в фаерволе микротика.");

            var id = _mtCon.Command("/ip firewall filter add")
                .Attribute("chain", "forward")
                .Attribute("dst-address", _thisMachineIp.ToString())
                .Attribute("protocol", "tcp")
                .Attribute("dst-port", $"{dstPort}")
                //.Attribute("in-interface", _config.WanIface)
                .Attribute("action", "accept")
                .Attribute("disabled", "true")
                .Attribute("comment", "Let's Encrypt challenge")
                .Scalar<string>();

            Log.Information("Получаем список всех статичных правил фаервола микротика.");

            // Список всех правил.
            var ids = _mtCon.Command("/ip firewall filter print")
                .Query("dynamic", "false")
                .Proplist(".id")
                .ScalarArray<string>();

            // Передвинуть правило в самое начало.
            if (ids.Length > 1)
            {
                Log.Information("Перемещаем наше правило фаервола в самое начало.");

                _mtCon.Command("/ip firewall filter move")
                    .Attribute("numbers", id) // что переместить.
                    .Attribute("destination", ids[0]) // перед чем.
                    .Send();
            }

            Log.Information("Включаем правило фаервола.");

            // Включить правило.
            _mtCon.Command("/ip firewall filter set")
                .Attribute(".id", id)
                .Attribute("disabled", "false")
                .Send();

            return id;
        }

        private string MtAllowMangleRule(int dstPort)
        {
            Log.Information($"Создаём правило мангла для прямой маршрутизации порта {dstPort}.");

            var id = _mtCon.Command("/ip firewall mangle add")
                .Attribute("chain", "prerouting")
                .Attribute("src-address", _thisMachineIp.ToString())
                .Attribute("protocol", "tcp")
                .Attribute("src-port", dstPort.ToString(CultureInfo.InvariantCulture))
                .Attribute("action", "accept")
                .Attribute("passthrough", "no")
                .Attribute("disabled", "true")
                .Attribute("comment", "Let's Encrypt challenge")
                .Scalar<string>();

            Log.Information("Получаем список всех статичных правил мангла.");

            // Список всех правил.
            var ids = _mtCon.Command("/ip firewall mangle print")
                .Query("dynamic", "false")
                .Proplist(".id")
                .ScalarArray<string>();

            // Передвинуть правило в самое начало.
            if (ids.Length > 1)
            {
                Log.Information("Перемещаем наше правило мангла в самое начало.");

                _mtCon.Command("/ip firewall mangle move")
                    .Attribute("numbers", id) // что переместить.
                    .Attribute("destination", ids[0]) // перед чем.
                    .Send();
            }

            Log.Information("Включаем правило мангла.");

            // Включить правило.
            _mtCon.Command("/ip firewall mangle set")
                .Attribute(".id", id)
                .Attribute("disabled", "false")
                .Send();

            return id;
        }

        private void ClosePort(string ruleId)
        {
            Log.Information("Удаляем созданное правило фаервола.");

            // Удалить правило.
            _mtCon.Command("/ip firewall filter remove")
                .Attribute(".id", ruleId)
                .Send();
        }

        /// <summary>
        /// Перенастраивает NAT для доступа извне к указанному порту.
        /// </summary>
        private string MtAddDstNatRule(int dstPort, int toPorts)
        {
            Log.Information("Создаём выключенное правило NAT в микротике.");

            var ruleId = _mtCon.Command("/ip firewall nat add")
                .Attribute("chain", "dstnat")
                .Attribute("protocol", "tcp")
                .Attribute("dst-port", $"{dstPort}")
                //.Attribute("in-interface", _config.WanIface)
                .Attribute("action", "netmap")
                .Attribute("to-addresses", _thisMachineIp.ToString())
                .Attribute("to-ports", $"{toPorts}")
                .Attribute("disabled", "true")
                .Attribute("comment", "Let's Encrypt challenge")
                .Scalar<string>();

            Log.Information("Получаем список всех статичных правил NAT микротика.");

            // Список всех правил.
            var ids = _mtCon.Command("/ip firewall nat print")
                .Query("dynamic", "false")
                .Proplist(".id")
                .ScalarArray<string>();

            // Передвинуть правило в самое начало.
            if (ids.Length > 1)
            {
                Log.Information("Перемещаем наше правило NAT в самое начало.");

                _mtCon.Command("/ip firewall nat move")
                    .Attribute("numbers", ruleId) // что переместить.
                    .Attribute("destination", ids[0]) // перед чем.
                    .Send();
            }

            Log.Information("Включаем правило NAT.");

            // Включить правило.
            _mtCon.Command("/ip firewall nat set")
                .Attribute(".id", ruleId)
                .Attribute("disabled", "false")
                .Send();

            return ruleId;
        }

        private void RemoveNatRule(string ruleId)
        {
            Log.Information("Удаляем созданное правило NAT.");

            // Удалить правило.
            _mtCon.Command("/ip firewall nat remove")
                .Attribute(".id", ruleId)
                .Send();
        }

        private void RemoveMangleRule(string ruleId)
        {
            Log.Information("Удаляем созданное правило мангла.");

            // Удалить правило.
            _mtCon.Command("/ip firewall mangle remove")
                .Attribute(".id", ruleId)
                .Send();
        }
    }
}
