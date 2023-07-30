namespace LetsEncryptMikroTik.Core;

public enum VerificationMethod
{
    None,
    HTTP01,
    DNS01,
    TLSALPN01
}
