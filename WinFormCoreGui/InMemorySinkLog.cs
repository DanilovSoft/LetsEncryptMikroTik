using System;

namespace LetsEncryptMikroTik.WinForm;

internal sealed class InMemorySinkLog : InMemorySink
{
    public event EventHandler<string>? NewMessage;

    public override void NewEntry(string message)
    {
        NewMessage?.Invoke(this, message);
    }
}
