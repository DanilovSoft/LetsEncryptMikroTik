using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting.Display;
using System.Globalization;
using System.IO;

namespace LetsEncryptMikroTik.WinForm;

public abstract class InMemorySink : ILogEventSink
{
    private readonly MessageTemplateTextFormatter _textFormatter = new("[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}", CultureInfo.CurrentUICulture);

    public void Emit(LogEvent logEvent)
    {
        var message = RenderLogEntry(logEvent);
        NewEntry(message);
    }

    private string RenderLogEntry(LogEvent logEvent)
    {
        using var writer = new StringWriter();
        _textFormatter.Format(logEvent, writer);
        return writer.ToString();
    }

    public abstract void NewEntry(string message);
}
