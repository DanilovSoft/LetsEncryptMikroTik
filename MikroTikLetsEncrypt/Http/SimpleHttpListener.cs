using System.Diagnostics;
using System.Net.Sockets;
using System.Text;
using System.Threading.Channels;

namespace System.Net;

internal sealed class SimpleHttpListener : IDisposable
{
    private readonly List<(TcpClient, Task)> _clients = new();
    private readonly TaskCompletionSource<int> _tcs = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly TcpListener _listener;
    private readonly Channel<MyHttpListenerContext> _channel;
    private readonly IPAddress _localAddress;
    private volatile bool _stopping;
    private int? _listenPort;

    public SimpleHttpListener(IPAddress localAddress)
    {
        ArgumentNullException.ThrowIfNull(localAddress);

        _localAddress = localAddress;
        _listener = new TcpListener(localAddress, 0);

        _channel = Channel.CreateUnbounded<MyHttpListenerContext>(new UnboundedChannelOptions
        {
            AllowSynchronousContinuations = false,
            SingleReader = true,
            SingleWriter = true,
        });
    }

    public int ListenPort => _listenPort ?? throw new InvalidOperationException("Сначала нужно вызвать Listen");

    public void Listen()
    {
        _listener.Start();
        _listenPort = ((IPEndPoint)_listener.LocalEndpoint).Port;
    }

    public async Task<bool> StopAsync(TimeSpan timeSpan)
    {
        (TcpClient, Task)[] copy;
        lock (_clients)
        {
            if (!_stopping)
            {
                _stopping = true;
                copy = _clients.ToArray();
            }
            else
                return true;
        }

        // Больше не принимать новых соединений.
        _listener.Stop();
        _channel.Writer.TryComplete();

        var whenAll = Task.WhenAll(copy.Select(x => x.Item2));

        if (await Task.WhenAny(whenAll, Task.Delay(timeSpan)).ConfigureAwait(false) != whenAll)
        {
            foreach (var item in copy)
            {
                item.Item1.Client.Shutdown(SocketShutdown.Both);
            }
        }

        await _tcs.Task.ConfigureAwait(false);

        // TODO
        return true;
    }

    public void Dispose()
    {
        if (!_stopping)
        {
            _listener.Stop();
        }
    }

    /// <summary>
    /// Возвращает <see langword="null"/> если слушатель остановлен.
    /// </summary>
    public async ValueTask<MyHttpListenerContext?> GetContextAsync()
    {
        if (await _channel.Reader.WaitToReadAsync().ConfigureAwait(false))
        {
            _channel.Reader.TryRead(out var context);
            return context;
        }
        else
            return null;
    }

    public void Start()
    {
        _listener.BeginAcceptTcpClient(OnConnection, null);
    }

    private void OnConnection(IAsyncResult result)
    {
        if (!_stopping)
        {
            TcpClient client;
            try
            {
                client = _listener.EndAcceptTcpClient(result);
            }
            catch (Exception)
            // Сюда теоритически не можем попасть.
            {
                _tcs.TrySetResult(1);
                return;
            }

            _listener.BeginAcceptTcpClient(OnConnection, null);
            HandleClient(client);
        }
        else
        {
            _tcs.TrySetResult(0);
        }
    }

    private async void HandleClient(TcpClient client)
    {
        Task clientTask;
        (TcpClient client, Task task) tuple;

        lock (_clients)
        {
            if (!_stopping)
            {
                clientTask = HandleClientAsync(client);
                tuple = (client, clientTask);
                _clients.Add(tuple);
            }
            else
            {
                client.Dispose();
                return;
            }
        }

        // Не бросает исключения.
        await clientTask.ConfigureAwait(false);

        lock (_clients)
        {
            _clients.Remove(tuple);
        }
    }

    private async Task HandleClientAsync(TcpClient client)
    {
        try
        {
            using (client)
            using (var stream = client.GetStream())
            {
                while (true)
                {
                    var request = await ReadHeadersAsync(stream).ConfigureAwait(false);
                    if (request == null)
                        return;

                    _channel.Writer.TryWrite(new MyHttpListenerContext(stream, request));
                }
            }
        }
        catch when (_stopping)
        {
            return;
        }
        catch (Exception)
        {
            Debug.WriteLine("Обрыв соединения.");
        }
    }

    private async ValueTask<MyHttpListenerRequest?> ReadHeadersAsync(Stream stream)
    {
        using (var reader = new StreamReader(stream, Encoding.ASCII, detectEncodingFromByteOrderMarks: true, bufferSize: 1024, leaveOpen: true))
        {
            string? line;
            string? headerName = null;

            // Null если конец потока.
            var initLine = await reader.ReadLineAsync().ConfigureAwait(false);
            if (initLine == null)
                return null;

            var split = initLine.Split(' ');
            var method = split[0];
            var request = split[1];

            var headers = new Dictionary<string, string>(5, StringComparer.InvariantCultureIgnoreCase);
            do
            {
                line = await reader.ReadLineAsync().ConfigureAwait(false);
                if (line == null)
                    return null;

                if (line.Length == 0)
                    break;

                var ind = line.IndexOf(':', StringComparison.Ordinal);
                if (ind != -1)
                {
                    headerName = line[..ind].Trim();
                    var value = line[(ind + 1)..].Trim();

                    if (value.Length > 0 && headers.TryGetValue(headerName, out var val))
                    {
                        headers[headerName] = val + "," + value;
                    }
                    else
                    {
                        headers.Add(headerName, value);
                    }
                }
                else
                {
                    if (headerName != null)
                    {
                        line = line.Trim();
                        headers[headerName] += " " + line;
                    }
                }
            } while (true);

            Uri uri;
            if (headers.TryGetValue("host", out var host))
            {
                uri = new Uri($"http://{host}{request}", UriKind.Absolute);
            }
            else
            {
                uri = new Uri($"http://{_localAddress}:{ListenPort}{request}", UriKind.Absolute);
            }

            return new MyHttpListenerRequest(method, uri, headers);
        }
    }
}
