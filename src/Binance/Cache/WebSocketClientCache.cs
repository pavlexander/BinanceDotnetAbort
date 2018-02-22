﻿using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Binance.Api;
using Binance.Cache.Events;
using Binance.WebSocket.Events;
using Microsoft.Extensions.Logging;

namespace Binance.Cache
{
    public abstract class WebSocketClientCache<TClient, TEventArgs, TCacheEventArgs>
        where TClient : class
        where TEventArgs : ClientEventArgs
        where TCacheEventArgs : CacheEventArgs
    {
        #region Public Events

        public event EventHandler<TCacheEventArgs> Update;

        #endregion Public Events

        #region Public Properties

        public TClient Client { get; private set; }

        #endregion Public Properties

        #region Protected Fields

        protected readonly IBinanceApi Api;

        protected readonly ILogger Logger;

        #endregion Protected Fields

        #region Private Fields

        private Action<TCacheEventArgs> _callback;

        private BufferBlock<TEventArgs> _bufferBlock;
        private ActionBlock<TEventArgs> _actionBlock;

        private bool _isLinked;

        #endregion Private Fields

        #region Constructors

        protected WebSocketClientCache(IBinanceApi api, TClient client, ILogger logger = null)
        {
            Throw.IfNull(api, nameof(api));
            Throw.IfNull(client, nameof(client));

            Api = api;
            Client = client;
            Logger = logger;
        }

        #endregion Constructors

        #region Public Methods

        public virtual void LinkTo(TClient client, Action<TCacheEventArgs> callback = null)
        {
            Throw.IfNull(client, nameof(client));

            if (_isLinked)
            {
                if (client == Client)
                    return; // ignore.

                throw new InvalidOperationException($"{GetType().Name} is linked to another {Client.GetType().Name}.");
            }

            Client = client;

            _isLinked = true;

            _callback = callback;

            _bufferBlock = new BufferBlock<TEventArgs>(new DataflowBlockOptions
            {
                EnsureOrdered = true,
                CancellationToken = CancellationToken.None,
                BoundedCapacity = DataflowBlockOptions.Unbounded,
                MaxMessagesPerTask = DataflowBlockOptions.Unbounded
            });

            _actionBlock = new ActionBlock<TEventArgs>(async @event =>
            {
                TCacheEventArgs eventArgs = null;

                try
                {
                    eventArgs = await OnAction(@event)
                        .ConfigureAwait(false);
                }
                catch (OperationCanceledException) { /* ignore */ }
                catch (Exception e)
                {
                    Logger?.LogError(e, $"{GetType().Name}: Unhandled {nameof(OnAction)} exception.  [thread: {Thread.CurrentThread.ManagedThreadId}{(@event.Token.IsCancellationRequested ? ", canceled" : string.Empty)}]");
                }

                if (eventArgs != null)
                {
                    try
                    {
                        _callback?.Invoke(eventArgs);
                        Update?.Invoke(this, eventArgs);
                    }
                    catch (OperationCanceledException) { /* ignore */ }
                    catch (Exception e)
                    {
                        Logger?.LogError(e, $"{GetType().Name}: Unhandled update event handler exception.  [thread: {Thread.CurrentThread.ManagedThreadId}{(@event.Token.IsCancellationRequested ? ", canceled" : string.Empty)}]");
                    }
                }
            }, new ExecutionDataflowBlockOptions
            {
                BoundedCapacity = 1,
                EnsureOrdered = true,
                //MaxMessagesPerTask = 1,
                MaxDegreeOfParallelism = 1,
                CancellationToken = CancellationToken.None,
                SingleProducerConstrained = true
            });

            _bufferBlock.LinkTo(_actionBlock);
        }

        public virtual void UnLink()
        {
            _callback = null;

            _isLinked = false;

            _bufferBlock?.Complete();
            _actionBlock?.Complete();
        }

        #endregion Public Methods

        #region Protected Methods

        /// <summary>
        /// Abstract event action handler.
        /// </summary>
        /// <param name="event"></param>
        /// <returns></returns>
        protected abstract ValueTask<TCacheEventArgs> OnAction(TEventArgs @event);

        /// <summary>
        /// Route event handler to callback method.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="event"></param>
        protected void OnClientEvent(object sender, TEventArgs @event)
            => ClientCallback(@event);

        /// <summary>
        /// Handle client event (provides buffering and single-threaded execution).
        /// </summary>
        /// <param name="event"></param>
        protected void ClientCallback(TEventArgs @event)
            => _bufferBlock.Post(@event);

        #endregion Protected Methods
    }
}
