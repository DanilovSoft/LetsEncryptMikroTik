using System.Net;

namespace LetsEncryptMikroTik.Core;

public sealed class Options
{
    internal const int ListenPort = 443; // The HTTP-01 challenge can only be done on port 80.
    public required string MikroTikAddress { get; init; }
    public required int MikroTikPort { get; init; }
    public required string DomainName { get; init; }
    public required string MikroTikLogin { get; init; }
    public required string WanIface { get; init; }
    public required LeUri LetsEncryptAddress { get; init; }
    public string? FtpLogin { get; init; }
    public string? FtpPassword { get; init; }
    public string? MikroTikPassword { get; init; }
    public string? Email { get; init; }
    public bool Force { get; init; }
    internal int ReplaceCertOnDaysLessThan { get; init; } = 60;
    public bool SaveFile { get; init; }
    public bool UseAlpn { get; init; }
    internal IPAddress? ThisMachineIp { get; set; }
}
