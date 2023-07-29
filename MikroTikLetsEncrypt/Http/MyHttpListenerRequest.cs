namespace System.Net;

internal sealed class MyHttpListenerRequest
{
    public Dictionary<string, string> Headers { get; }
    public string Method { get; }
    public Uri Uri { get; }

    public MyHttpListenerRequest(string method, Uri uri, Dictionary<string, string> headers)
    {
        Headers = headers;
        Method = method;
        Uri = uri;
    }
}
