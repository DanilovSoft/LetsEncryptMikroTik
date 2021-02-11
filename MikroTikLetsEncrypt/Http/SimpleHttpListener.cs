using LetsEncryptMikroTik.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace System.Net
{
    internal sealed class SimpleHttpListener : IDisposable
    {
        private const int PreferedPort = 80;
        private readonly List<(TcpClient, Task)> _clients = new List<(TcpClient, Task)>();
        private readonly TaskCompletionSource<int> _tcs = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
        public int ListenPort { get; internal set; }
        private readonly TcpListener _listener;
        private readonly Channel<MyHttpListenerContext> _channel;
        private readonly IPAddress _address;
        private volatile bool _stopping;

        public SimpleHttpListener(IPAddress thisMachineIp)
        {
            _address = thisMachineIp;
            int prefPort = PreferedPort;
            bool started = false;
            do
            {
                int listenPort = FindAvailablePort(thisMachineIp, prefPort);
                var tcp = new TcpListener(thisMachineIp, listenPort);
                try
                {
                    tcp.Start();
                }
                catch (SocketException ex) when (ex.SocketErrorCode == SocketError.AddressAlreadyInUse)
                {
                    prefPort = listenPort + 1;
                    continue;
                }
                started = true;
                ListenPort = listenPort;
                _listener = tcp;
                
            } while (!started);

            _channel = Channel.CreateUnbounded<MyHttpListenerContext>(new UnboundedChannelOptions
            {
                AllowSynchronousContinuations = false,
                SingleReader = true,
                SingleWriter = true,
            });
        }

        // Находит не занятый порт.
        private static int FindAvailablePort(IPAddress address, int prefPort)
        {
            // Evaluate current system tcp connections. This is the same information provided
            // by the netstat command line application, just in .Net strongly-typed object
            // form.  We will look through the list, and if our port we would like to use
            // in our TcpClient is occupied, we will set isAvailable to false.
            IPGlobalProperties ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
            //TcpConnectionInformation[] tcpConnInfoArray = ipGlobalProperties.GetActiveTcpConnections();
            IPEndPoint[] listeners = ipGlobalProperties.GetActiveTcpListeners();

            while (listeners.Any(x => x.Address.Equals(address) && x.Port == prefPort))
            {
                checked
                {
                    prefPort += 1;
                }
            }
            return prefPort;
        }

        /// <summary>
        /// Возвращает <see langword="null"/> если слушатель остановлен.
        /// </summary>
        public async ValueTask<MyHttpListenerContext?> GetContextAsync()
        {
            if (await _channel.Reader.WaitToReadAsync().ConfigureAwait(false))
            {
                _channel.Reader.TryRead(out MyHttpListenerContext context);
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
                using (NetworkStream stream = client.GetStream())
                {
                    while (true)
                    {
                        MyHttpListenerRequest? request = await ReadHeadersAsync(stream).ConfigureAwait(false);
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
                string? initLine = await reader.ReadLineAsync().ConfigureAwait(false);
                if (initLine == null)
                    return null;

                string[] split = initLine.Split(' ');
                string method = split[0];
                string request = split[1];

                var headers = new Dictionary<string, string>(5, StringComparer.InvariantCultureIgnoreCase);
                do
                {
                    line = await reader.ReadLineAsync().ConfigureAwait(false);
                    if (line == null)
                        return null;

                    if (line.Length == 0)
                        break;

                    int ind = line.IndexOf(':', StringComparison.Ordinal);
                    if (ind != -1)
                    {
                        headerName = line.Substring(0, ind).Trim();
                        var value = line.Substring(ind + 1).Trim();

                        if (value.Length > 0 && headers.TryGetValue(headerName, out string? val))
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
                        line = line.Trim();
                        headers[headerName] += " " + line;
                    }
                } while (true);

                Uri uri;
                if (headers.TryGetValue("host", out string? host))
                {
                    uri = new Uri($"http://{host}{request}", UriKind.Absolute);
                }
                else
                {
                    uri = new Uri($"http://{_address}:{ListenPort}{request}", UriKind.Absolute);
                }

                return new MyHttpListenerRequest(method, uri, headers);
            }
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

            if(await Task.WhenAny(whenAll, Task.Delay(timeSpan)).ConfigureAwait(false) != whenAll)
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
    }
}
