﻿using System;
using System.Collections.Generic;
using Binance.Cache.Events;
using Binance.Market;
using Binance.WebSocket;

namespace Binance.Cache
{
    public interface IAggregateTradeCache
    {
        #region Events

        /// <summary>
        /// Aggregate trades update event.
        /// </summary>
        event EventHandler<AggregateTradeCacheEventArgs> Update;

        /// <summary>
        /// Aggregate trades out-of-sync event.
        /// </summary>
        event EventHandler<EventArgs> OutOfSync;

        #endregion Events

        #region Properties

        /// <summary>
        /// The latest trades. Can be empty if not yet synchronized or out-of-sync.
        /// </summary>
        IEnumerable<AggregateTrade> Trades { get; }

        /// <summary>
        /// The client that provides trade information.
        /// </summary>
        IAggregateTradeWebSocketClient Client { get; }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Subscribe to a symbol.
        /// </summary>
        /// <param name="symbol">The symbol.</param>
        /// <param name="limit">The number of trades to cache.</param>
        /// <param name="callback">The callback (optional).</param>
        void Subscribe(string symbol, int limit, Action<AggregateTradeCacheEventArgs> callback);

        /// <summary>
        /// Unsubscribe from the currently subscribed symbol.
        /// </summary>
        void Unsubscribe();

        /// <summary>
        /// Link to a subscribed <see cref="IAggregateTradeWebSocketClient"/>.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="callback"></param>
        void LinkTo(IAggregateTradeWebSocketClient client, Action<AggregateTradeCacheEventArgs> callback = null);

        /// <summary>
        /// Unlink from client.
        /// </summary>
        void UnLink();

        #endregion Methods
    }
}
