using DanilovSoft.MikroApi;
using System.Runtime.Serialization;

namespace LetsEncryptMikroTik.Core;

internal struct IpService
{
    [MikroTikProperty("port")]
    public int Port { get; set; }

    [MikroTikProperty("disabled")]
    public bool Disabled { get; set; }

    [MikroTikProperty("address")]
    public string Address { get; set; }

    private string[]? _addresses;
    public string[] Addresses => _addresses ??= Address.Split(',', StringSplitOptions.TrimEntries);
}
