﻿using System.Net;
using System.Text;
using Microsoft.Extensions.Logging;

namespace LetsEncryptMikroTik.Core;

internal sealed class Http01ChallengeHandler : IChallengeHandler, IDisposable
{
    private readonly SimpleHttpListener _listener;
    private readonly string _keyAuthString;
    private readonly ILogger _logger;
    private volatile bool _stopRequired;
    private readonly TaskCompletionSource<int> _tcs = new(TaskCreationOptions.RunContinuationsAsynchronously);

    public Http01ChallengeHandler(IPAddress localAddress, string keyAuthString, ILogger logger)
    {
        ArgumentNullException.ThrowIfNull(localAddress);
        ArgumentNullException.ThrowIfNull(keyAuthString);

        _keyAuthString = keyAuthString;
        _logger = logger;
        _listener = new SimpleHttpListener(localAddress);
        _listener.Listen();
    }

    public int PublicPort => 80; // Не менять.
    public int ListenPort => _listener.ListenPort;
    public Task RequestHandled => _tcs.Task;

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
                var context = await _listener.GetContextAsync().ConfigureAwait(false);
                if (context == null)
                    return; // Слушатель был остановлен.

                if (context.Request.Method != "GET" || !context.Request.Uri.LocalPath.StartsWith("/.well-known/acme-challenge/", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                _logger.LogInformation("Принят HTTP запрос: {Scheme}://{Host}/...", context.Request.Uri.Scheme, context.Request.Uri.Host);

                var response = context.Response;
                response.ContentType = "plain/text";

                var content = Encoding.ASCII.GetBytes(_keyAuthString);
                response.ContentLength64 = content.Length;

                using (response.OutputStream)
                {
                    await response.OutputStream.WriteAsync(content).ConfigureAwait(false);
                    await response.OutputStream.CloseAsync().ConfigureAwait(false);
                }

                _tcs.TrySetResult(0);

                _logger.LogInformation("Отправлен ключ подтверждения");
            }
            catch (HttpListenerClosedException) // Грациозная остановка.
            {
                break;
            }
            catch when (_stopRequired) // Грязная остановка.
            {
                break;
            }
        }
    }

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