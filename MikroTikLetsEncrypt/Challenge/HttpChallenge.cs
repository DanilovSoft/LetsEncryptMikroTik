using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LetsEncryptMikroTik.Core
{
    internal sealed class HttpChallenge : IChallenge, IDisposable
    {
        private readonly SimpleHttpListener _listener;
        private readonly string _keyAuthString;
        private volatile bool _stopRequired;
        public int ListenPort => _listener.ListenPort;
        private readonly TaskCompletionSource<int> _tcs = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
        public Task Completion => _tcs.Task;

        public int PublicPort => 80; // Не менять.

        public HttpChallenge(ConfigClass config, string keyAuthString)
        {
            _keyAuthString = keyAuthString;
            _listener = new SimpleHttpListener(config.ThisMachineIp);
        }

        public void Start()
        {
            throw new NotImplementedException();
        }

        public async Task RunAsync()
        {
            _listener.Start();

            while (!_stopRequired)
            {
                try
                {
                    MyHttpListenerContext context = await _listener.GetContextAsync().ConfigureAwait(false);
                    if (context == null)
                        return; // Слушатель был остановлен.

                    if (context.Request.Method == "GET" && context.Request.Uri.LocalPath.StartsWith("/.well-known/acme-challenge/"))
                    {
                        Log.Information($"Принят HTTP запрос: {context.Request.Uri.Scheme}://{context.Request.Uri.Host}/...");

                        MyHttpListenerResponse response = context.Response;
                        response.ContentType = "plain/text";

                        byte[] content = Encoding.ASCII.GetBytes(_keyAuthString);
                        response.ContentLength64 = content.Length;

                        using (response.OutputStream)
                        {
                            await response.OutputStream.WriteAsync(content, 0, content.Length).ConfigureAwait(false);
                            await response.OutputStream.CloseAsync().ConfigureAwait(false);
                        }

                        _tcs.TrySetResult(0);

                        Log.Information($"Отправлен ключ подтверждения.");
                    }
                }
                catch (HttpListenerClosedException)
                {
                    // Грациозная остановка.
                    break;
                }
                catch when (_stopRequired)
                {
                    // Грязная остановка.
                    break;
                }
            }
        }

        // Для .Net Core пакет: Microsoft.AspNetCore.App
        //private IWebHostBuilder AspNetHandler(string keyAuthString, SemaphoreSlim sem)
        //{
        //    var refCopy = sem;

        //    var hostBuilder = WebHost.CreateDefaultBuilder()
        //        .ConfigureLogging(conf =>
        //        {
        //            conf.ClearProviders();
        //        })
        //        .Configure(app =>
        //        {
        //            // Handle Lets Encrypt Route.
        //            app.UseRouter(r =>
        //            {
        //                r.MapGet(".well-known/acme-challenge/{token}", async (request, response, routeData) =>
        //                {
        //                    string token = routeData.Values["token"] as string;
        //                    Log.Information($"Принят HTTP запрос. Token: {token}.");

        //                    response.ContentType = "plain/text";
        //                    await response.WriteAsync(keyAuthString);

        //                    Log.Information($"Отправлен ключ подтверждения: {keyAuthString}.");

        //                    Interlocked.CompareExchange(ref sem, null, refCopy)?.Release();
        //                });
        //            });
        //        })
        //        .ConfigureServices(conf =>
        //        {
        //            conf.AddRouting();
        //        })
        //        .UseUrls($"http://*:{_listenPort}");

        //    return hostBuilder;
        //}

        public Task<bool> StopAsync(TimeSpan timeSpan)
        {
            _stopRequired = true;
            return _listener.StopAsync(timeSpan);
        }

        public void Dispose()
        {
            _listener.Dispose();
        }
    }
}
