using System.Net;

namespace LetsEncryptMikroTik.Core.Challenge;

internal sealed class Dns01ChallengeHandler : IChallengeHandler, IDisposable
{
    private readonly IPAddress _localIP;
    private readonly string _domainName;
    private readonly string _keyAuthString;

    public Dns01ChallengeHandler(IPAddress localIP, string domainName, string keyAuthString)
    {
        ArgumentNullException.ThrowIfNull(localIP);
        ArgumentException.ThrowIfNullOrEmpty(domainName);
        ArgumentException.ThrowIfNullOrEmpty(keyAuthString);
        
        _localIP = localIP;
        _domainName = domainName;
        _keyAuthString = keyAuthString;
    }

    public int PublicPort => 53;
    public Task RequestHandled => throw new NotImplementedException();
    public int ListenPort => throw new NotImplementedException();

    public void Dispose()
    {
    }

    public void Start()
    {
    }
}
