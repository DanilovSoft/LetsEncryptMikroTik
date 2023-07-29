namespace LetsEncryptMikroTik.Core;

public sealed class CertUpdaterOptions
{
    public CertUpdaterOptions()
    {
        
    }

    public required string WanIface { get; init; }
    public required string CommonName { get; init; }
    public required string DomainName { get; init; }
}
