using System.IO;

namespace System.Net;

internal sealed class MyHttpListenerContext
{
    private readonly Stream _stream;
    public MyHttpListenerResponse Response { get; }
    public MyHttpListenerRequest Request { get; }

    public MyHttpListenerContext(Stream stream, MyHttpListenerRequest request)
    {
        _stream = stream;
        Request = request;
        Response = new MyHttpListenerResponse(stream);
    }
}
