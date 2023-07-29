using System.Net;

namespace LetsEncryptMikroTik.Core;

public sealed class CertUpdaterOptions
{
    /// <summary>
    /// IP Address or hostname.
    /// </summary>
    public required string MikroTikAddress { get; init; }
    public required string WanIface { get; init; }
    public required string DomainName { get; init; }
    /// <summary>
    /// IP Address to use for local server.
    /// </summary>
    public required IPAddress LocalIP { get; init; }
    public required LeUri LetsEncryptAddress { get; init; }
    public string? FtpLogin { get; init; }
    public string? FtpPassword { get; init; }
    //public string? MikroTikPassword { get; init; }
    public string? Email { get; init; }
    public bool Force { get; init; }
    public bool SaveFile { get; init; }
    public bool UseAlpn { get; init; }
    public int ReplaceCertOnDaysLessThan { get; init; } = 60;
}
