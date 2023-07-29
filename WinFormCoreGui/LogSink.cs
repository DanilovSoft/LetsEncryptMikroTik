using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Formatting.Display;
using System.Globalization;
using System.IO;

namespace LetsEncryptMikroTik.WinForm;

public abstract class InMemorySink : ILogEventSink
{
    private readonly ITextFormatter _textFormatter = new MessageTemplateTextFormatter("[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}", CultureInfo.CurrentUICulture);

    public void Emit(LogEvent logEvent)
    {
        string message;
        using (var renderSpace = new StringWriter())
        {
            _textFormatter.Format(logEvent, renderSpace);
            message = renderSpace.ToString();
        }
        NewEntry(message);
    }

    public abstract void NewEntry(string message);
}
