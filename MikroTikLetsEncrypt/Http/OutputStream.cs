using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace System.Net;

internal sealed class OutputStream : Stream
{
    //private static readonly byte[] Fin = new byte[] { 10, 13, 10, 13 };
    private readonly Stream _stream;
    private readonly MyHttpListenerResponse _response;
    private bool _firstWrite = true;
    private bool _closed;

    public OutputStream(Stream stream, MyHttpListenerResponse response)
    {
        _stream = stream;
        _response = response;
    }

    public override bool CanRead => false;

    public override bool CanSeek => false;

    public override bool CanWrite => true;

    public override long Length => throw new NotSupportedException();

    public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }

    public override void Flush()
    {
        _stream.Flush();
    }

    public override Task FlushAsync(CancellationToken cancellationToken)
    {
        return _stream.FlushAsync(cancellationToken);
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        throw new NotSupportedException();
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        throw new NotSupportedException();
    }

    public override void SetLength(long value)
    {
        throw new NotSupportedException();
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        WriteAsync(buffer, offset, count, CancellationToken.None).GetAwaiter().GetResult();
    }

    public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        if (_firstWrite)
        {
            _firstWrite = false;
            await WriteHeadersAsync().ConfigureAwait(false);
        }

        await _stream.WriteAsync(buffer, offset, count, cancellationToken).ConfigureAwait(false);
    }

    private async Task WriteHeadersAsync()
    {
        var sb = new StringBuilder($"HTTP/1.1 {_response.StatusCode} {MyHttpWorkerRequest.GetStatusDescription(_response.StatusCode)}");
        sb.AppendLine();
        sb.AppendLine($"Content-Length: {_response.ContentLength64}");

        if(!string.IsNullOrEmpty(_response.ContentType))
        {
            sb.AppendLine($"Content-Type: {_response.ContentType}");
        }

        sb.AppendLine();
        var headers = Encoding.ASCII.GetBytes(sb.ToString());
        await _stream.WriteAsync(headers, 0, headers.Length).ConfigureAwait(false);
    }

    public override void Close()
    {
        if (!_closed)
        {
            _closed = true;
            CloseAsync().GetAwaiter().GetResult();
            // Flush сделает Dispose.
        }
    }

    public async Task CloseAsync()
    {
        if (!_closed)
        {
            _closed = true;
            //await _stream.WriteAsync(Fin, 0, 4).ConfigureAwait(false);
            await FlushAsync().ConfigureAwait(false);
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            Flush();
        }
    }
}
