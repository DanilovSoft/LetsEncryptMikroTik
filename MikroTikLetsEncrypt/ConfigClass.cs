using System;
using System.Diagnostics;
using System.Net;

namespace LetsEncryptMikroTik.Core;

public class ConfigClass
{
    // The HTTP-01 challenge can only be done on port 80.
    internal const int ListenPort = 443;
    public int MikroTikPort { get; set; }
    public string? FtpLogin { get; set; }
    public string? FtpPassword { get; set; }
    public string? MikroTikLogin { get; set; }
    public string? MikroTikPassword { get; set; }
    public string? DomainName { get; set; }
    public string? Email { get; set; }
    public string? WanIface { get; set; }
    public bool Force { get; set; }
    public LeUri? LetsEncryptAddress { get; set; }
    public string? MikroTikAddress { get; set; }
    internal int ReplaceCertOnDaysLessThan { get; set; } = 60;
    internal IPAddress? ThisMachineIp { get; set; }
    public bool SaveFile { get; set; }
    public bool UseAlpn { get; set; }
}

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
