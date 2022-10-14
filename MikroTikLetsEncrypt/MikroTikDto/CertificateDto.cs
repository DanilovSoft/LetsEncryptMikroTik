using DanilovSoft.MikroApi;

namespace LetsEncryptMikroTik.Core;

internal sealed class CertificateDto
{
    [MikroTikProperty(".id")]
    public string? Id { get; set; }

    [MikroTikProperty("name")]
    public string? Name { get; set; }

    [MikroTikProperty("invalid-after")]
    public string? InvalidAfter { get; set; }
}
