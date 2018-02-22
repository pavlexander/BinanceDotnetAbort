﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Binance.WebSocket.Events;
using Microsoft.Extensions.Logging;

// ReSharper disable once CheckNamespace
namespace Binance.WebSocket
{
    public class WebSocket4NetClient : WebSocketClient
    {
        private volatile bool _isOpen;

        public WebSocket4NetClient(ILogger<WebSocket4NetClient> logger)
            : base(logger)
        { }

        public override async Task StreamAsync(Uri uri, CancellationToken token)
        {
            if (uri == null)
                throw new ArgumentNullException(nameof(uri));

            if (!token.CanBeCanceled)
                throw new ArgumentException("Token must be capable of being in the canceled state.", nameof(token));

            token.ThrowIfCancellationRequested();

            if (IsStreaming)
                throw new InvalidOperationException($"{nameof(WebSocket4NetClient)}.{nameof(StreamAsync)}: Already streaming (this method is not reentrant).");

            IsStreaming = true;

            Exception exception = null;

            var tcs = new TaskCompletionSource<object>();
            token.Register(() => tcs.TrySetCanceled());

            var webSocket = new WebSocket4Net.WebSocket(uri.AbsoluteUri);

            webSocket.Opened += (s, e) =>
            {
                _isOpen = true;
                RaiseOpenEvent();
            };

            webSocket.Closed += (s, e) => tcs.TrySetCanceled();

            webSocket.MessageReceived += (s, evt) =>
            {
                try
                {
                    var json = evt.Message;

                    if (!string.IsNullOrWhiteSpace(json))
                    {
                        RaiseMessageEvent(new WebSocketClientEventArgs(json));
                    }
                    else
                    {
                        Logger?.LogWarning($"{nameof(WebSocket4NetClient)}.MessageReceived: Received empty JSON message.");
                    }
                }
                catch (OperationCanceledException) { }
                catch (Exception e)
                {
                    if (!token.IsCancellationRequested)
                    {
                        Logger?.LogError(e, $"{nameof(WebSocket4NetClient)}.MessageReceived: WebSocket read exception.");
                        exception = e;
                        tcs.TrySetCanceled();
                    }
                }
            };

            webSocket.Error += (s, e) =>
            {
                if (token.IsCancellationRequested)
                    return;

                Logger?.LogError(e.Exception, $"{nameof(WebSocket4NetClient)}.Error: WebSocket exception.");
                exception = e.Exception;
                tcs.TrySetCanceled();
            };

            try
            {
                webSocket.Open();

                await tcs.Task
                    .ConfigureAwait(false);

                if (exception != null)
                    throw exception;
            }
            catch (OperationCanceledException) { }
            catch (Exception e)
            {
                if (!token.IsCancellationRequested)
                {
                    Logger?.LogError(e, $"{nameof(WebSocket4NetClient)}.{nameof(StreamAsync)}: WebSocket open exception.");
                    throw;
                }
            }
            finally
            {
                if (webSocket.State == WebSocket4Net.WebSocketState.Open)
                {
                    try { webSocket.Close(); }
                    catch (Exception e)
                    {
                        Logger?.LogError(e, $"{nameof(WebSocket4NetClient)}.{nameof(StreamAsync)}: WebSocket close exception.");
                    }
                }

                webSocket.Dispose();

                if (_isOpen)
                {
                    _isOpen = false;
                    RaiseCloseEvent();
                }

                IsStreaming = false;
            }
        }
    }
}
