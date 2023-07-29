using System.Runtime.Serialization;

namespace LetsEncryptMikroTik.Core;

public class LetsEncryptMikroTikException : Exception
{
    public LetsEncryptMikroTikException()
    {
    }

    public LetsEncryptMikroTikException(string? message) : base(message)
    {
    }

    public LetsEncryptMikroTikException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}
