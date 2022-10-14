using System;

namespace LetsEncryptMikroTik.Core;

[Serializable]
public sealed class HttpListenerClosedException : Exception
{
    public HttpListenerClosedException(string message) : base(message)
    {
    }

    public HttpListenerClosedException(string message, Exception innerException) : base(message, innerException)
    {
    }

    public HttpListenerClosedException()
    {
    }
}
