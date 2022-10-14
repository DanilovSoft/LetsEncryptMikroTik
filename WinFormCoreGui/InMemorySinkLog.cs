using LetsEncryptMikroTik.Core;

namespace LetsEncryptMikroTik;

internal class InMemorySinkLog : InMemorySink
{
    private readonly Form1 _form;

    public InMemorySinkLog(Form1 form)
    {
        _form = form;
    }

    public override void NewEntry(string message)
    {
        _form.OnLogMessage(message);
    }
}
