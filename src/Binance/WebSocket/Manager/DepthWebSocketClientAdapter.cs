﻿using System;
using System.Threading;
using Binance.WebSocket.Events;
using Microsoft.Extensions.Logging;
// ReSharper disable InconsistentlySynchronizedField

namespace Binance.WebSocket.Manager
{
    internal sealed class DepthWebSocketClientAdapter : BinanceWebSocketClientAdapter<IDepthWebSocketClient>, IDepthWebSocketClient
    {
        #region Public Events

        public event EventHandler<DepthUpdateEventArgs> DepthUpdate
        {
            add => Client.DepthUpdate += value;
            remove => Client.DepthUpdate -= value;
        }

        #endregion Public Events

        #region Constructors

        public DepthWebSocketClientAdapter(IBinanceWebSocketManager manager, IDepthWebSocketClient client, ILogger<IBinanceWebSocketManager> logger = null, Action<Exception> onError = null)
            : base(manager, client, logger, onError)
        { }

        #endregion Constructors

        #region Public Methods

        public void Subscribe(string symbol, int limit, Action<DepthUpdateEventArgs> callback)
        {
            lock (Sync)
            {
                Task = Task.ContinueWith(async _ =>
                {
                    try
                    {
                        Logger?.LogDebug($"{nameof(DepthWebSocketClientAdapter)}.{nameof(Subscribe)}: Cancel streaming...  [thread: {Thread.CurrentThread.ManagedThreadId}]");
                        await Controller.CancelAsync()
                            .ConfigureAwait(false);

                        Client.Subscribe(symbol, limit, callback);

                        if (!Manager.IsAutoStreamingDisabled && !Controller.IsActive)
                        {
                            Logger?.LogDebug($"{nameof(DepthWebSocketClientAdapter)}.{nameof(Subscribe)}: Begin streaming...  [thread: {Thread.CurrentThread.ManagedThreadId}]");
                            Controller.Begin();
                        }
                    }
                    catch (OperationCanceledException) { /* ignored */ }
                    catch (Exception e)
                    {
                        Logger?.LogError(e, $"{nameof(DepthWebSocketClientAdapter)}.{nameof(Subscribe)}: Failed.  [thread: {Thread.CurrentThread.ManagedThreadId}]");
                        OnError?.Invoke(e);
                    }
                });
            }
        }

        public void Unsubscribe(string symbol, int limit, Action<DepthUpdateEventArgs> callback)
        {
            lock (Sync)
            {
                Task = Task.ContinueWith(async _ =>
                {
                    try
                    {
                        Logger?.LogDebug($"{nameof(DepthWebSocketClientAdapter)}.{nameof(Unsubscribe)}: Cancel streaming...  [thread: {Thread.CurrentThread.ManagedThreadId}]");
                        await Controller.CancelAsync()
                            .ConfigureAwait(false);

                        Client.Unsubscribe(symbol, limit, callback);

                        if (!Manager.IsAutoStreamingDisabled && !Controller.IsActive)
                        {
                            Logger?.LogDebug($"{nameof(DepthWebSocketClientAdapter)}.{nameof(Unsubscribe)}: Begin streaming...  [thread: {Thread.CurrentThread.ManagedThreadId}]");
                            Controller.Begin();
                        }
                    }
                    catch (OperationCanceledException) { /* ignored */ }
                    catch (Exception e)
                    {
                        Logger?.LogError(e, $"{nameof(DepthWebSocketClientAdapter)}.{nameof(Unsubscribe)}: Failed.  [thread: {Thread.CurrentThread.ManagedThreadId}]");
                        OnError?.Invoke(e);
                    }
                });
            }
        }

        #endregion Public Methods
    }
}
