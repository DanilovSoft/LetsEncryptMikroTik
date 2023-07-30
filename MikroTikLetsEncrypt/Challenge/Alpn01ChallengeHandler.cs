using Org.BouncyCastle.Asn1;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace LetsEncryptMikroTik.Core.Challenge;

internal sealed class Alpn01ChallengeHandler : IChallengeHandler, IDisposable
{
    private readonly TcpListener _listener;
    private readonly string _identifier;
    private readonly X509Certificate2 _certificate;
    private readonly string _challengeTokenValue;
    private readonly CancellationTokenSource _cts = new();
    private readonly TaskCompletionSource<int> _tcs = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private int? _listenPort;

    public Alpn01ChallengeHandler(IPAddress localAddress, string identifier, string challengeTokenValue)
    {
        ArgumentNullException.ThrowIfNull(localAddress);
        ArgumentNullException.ThrowIfNull(identifier);
        ArgumentException.ThrowIfNullOrEmpty(challengeTokenValue);

        _identifier = identifier;
        _challengeTokenValue = challengeTokenValue;
        _certificate = PrepareChallenge();
        _listener = new TcpListener(localAddress, 0);
    }

    public int PublicPort => 443; // Не менять.
    public Task RequestHandled => _tcs.Task;
    public int ListenPort => _listenPort ?? throw new InvalidOperationException("Сначала нужно вызвать Start");

    public void Dispose()
    {
        _cts.Cancel();
        _listener.Stop();
        _certificate.Dispose();
    }

    public void Start()
    {
        _listener.Start();
        _listenPort = ((IPEndPoint)_listener.LocalEndpoint).Port;
        _listener.BeginAcceptTcpClient(OnConnected, null);
    }

    private X509Certificate2 PrepareChallenge()
    {
        try
        {
            using var rsa = RSA.Create(2048);
            var name = new X500DistinguishedName($"CN={_identifier}");

            var request = new CertificateRequest(
                name,
                rsa,
                HashAlgorithmName.SHA256,
                RSASignaturePadding.Pkcs1);

            var hash = HashChallenge(_challengeTokenValue);
            request.CertificateExtensions.Add(new X509Extension(new AsnEncodedData("1.3.6.1.5.5.7.1.31", new DerOctetString(hash).GetDerEncoded()), true));

            var sanBuilder = new SubjectAlternativeNameBuilder();
            sanBuilder.AddDnsName(_identifier);
            request.CertificateExtensions.Add(sanBuilder.Build());

            var certificate = request.CreateSelfSigned(
               new DateTimeOffset(DateTime.UtcNow.AddDays(-1)),
               new DateTimeOffset(DateTime.UtcNow.AddDays(1)));

            return new X509Certificate2(
                certificate.Export(X509ContentType.Pfx, _identifier),
                _identifier,
                X509KeyStorageFlags.MachineKeySet);
        }
        catch
        {
            //_log.Error("Unable to activate TcpClient, this may be because of insufficient rights or another application using port 443");
            throw;
        }
    }

    private static byte[] HashChallenge(string challenge)
    {
        Span<byte> decodedChallenge = stackalloc byte[256];
        var len = Encoding.UTF8.GetBytes(challenge, decodedChallenge);
        return SHA256.HashData(decodedChallenge[0..len]);
    }

    private void OnConnected(IAsyncResult asyncResult)
    {
        TcpClient client;
        try
        {
            client = _listener.EndAcceptTcpClient(asyncResult);
        }
        catch (ObjectDisposedException) // Был запрос на остановку.
        {
            return;
        }

        if (!_cts.IsCancellationRequested)
        {
            _listener.BeginAcceptTcpClient(OnConnected, null);
        }
        ProcessRequestAsync(client);
    }

    private async void ProcessRequestAsync(TcpClient client)
    {
        try
        {
            using (client)
            using (var sslStream = new SslStream(client.GetStream()))
            {
                var sslOptions = new SslServerAuthenticationOptions
                {
                    ApplicationProtocols = new List<SslApplicationProtocol>
                    {
                        new SslApplicationProtocol("acme-tls/1")
                    },
                    ServerCertificate = _certificate
                };

                await sslStream.AuthenticateAsServerAsync(sslOptions, _cts.Token).ConfigureAwait(false);

                // Дополнительная пауза перед отключением.
                await Task.Delay(1000).ConfigureAwait(false);

                _tcs.TrySetResult(0);
            }
        }
        catch (OperationCanceledException)
        {
            return;
        }
        catch (ObjectDisposedException)
        {
            return;
        }
        catch (Exception)
        {
            //Debug.Assert(false);
            //Debug.WriteLine(ex);
        }
    }
}