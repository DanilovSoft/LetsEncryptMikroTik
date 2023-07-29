namespace System.Net;

internal sealed class MyHttpListenerResponse
{
    public int StatusCode { get; set; } = 200;
    public OutputStream OutputStream { get; }
    public string ContentType { get; set; }
    public long ContentLength64 { get; set; }

    public MyHttpListenerResponse(Stream outputStream)
    {
        OutputStream = new OutputStream(outputStream, this);
    }
}
