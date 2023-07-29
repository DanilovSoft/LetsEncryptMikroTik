using System.Net;

namespace LetsEncryptMikroTik.Core;

public class ConfigClass
{
    // The HTTP-01 challenge can only be done on port 80.
    internal const int ListenPort = 443;
    public int MikroTikPort { get; init; }
    public string? FtpLogin { get; init; }
    public string? FtpPassword { get; init; }
    public string? MikroTikLogin { get; init; }
    public string? MikroTikPassword { get; init; }
    public string? DomainName { get; init; }
    public string? Email { get; init; }
    public string? WanIface { get; init; }
    public bool Force { get; init; }
    public LeUri? LetsEncryptAddress { get; init; }
    public string? MikroTikAddress { get; init; }
    internal int ReplaceCertOnDaysLessThan { get; init; } = 60;
    public bool SaveFile { get; init; }
    public bool UseAlpn { get; init; }
    internal IPAddress? ThisMachineIp { get; set; }
}
