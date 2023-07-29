using System.Diagnostics;

namespace LetsEncryptMikroTik.Core;

/// <summary>
/// Let's Encrypt Uri.
/// </summary>
[DebuggerDisplay("{_address}")]
public sealed class LeUri
{
    public static readonly LeUri StagingV2 = new(Certes.Acme.WellKnownServers.LetsEncryptStagingV2);
    public static readonly LeUri LetsEncryptV2 = new(Certes.Acme.WellKnownServers.LetsEncryptV2);
    private readonly Uri _address;

    private LeUri(Uri address)
    {
        _address = address;
    }

    public static implicit operator Uri(LeUri a)
    {
        return a._address;
    }

    public static explicit operator LeUri(Uri uri)
    {
        return new LeUri(uri);
    }
}
