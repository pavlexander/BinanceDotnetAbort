﻿using System;
using System.Threading;
using Binance.Api;
using Binance.Market;

namespace Binance.WebSocket.Events
{
    /// <summary>
    /// Candlestick/K-Line web socket client event.
    /// </summary>
    public sealed class CandlestickEventArgs : ClientEventArgs
    {
        #region Public Properties

        /// <summary>
        /// Get the candlestick.
        /// </summary>
        public Candlestick Candlestick { get; }

        /// <summary>
        /// Get the first trade ID. Can be <see cref="BinanceApi.NullId"/>.
        /// </summary>
        public long FirstTradeId { get; }

        /// <summary>
        /// Get the last trade ID. Can be <see cref="BinanceApi.NullId"/>.
        /// </summary>
        public long LastTradeId { get; }

        /// <summary>
        /// Get whether the candlestick is final.
        /// </summary>
        public bool IsFinal { get; }

        #endregion Public Properties

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="time">The event time.</param>
        /// <param name="token">The cancellation token.</param>
        /// <param name="candlestick">The candlestick.</param>
        /// <param name="firstTradeId">The first trade ID.</param>
        /// <param name="lastTradeId">The last trade ID.</param>
        /// <param name="isFinal">Is candlestick final.</param>
        public CandlestickEventArgs(DateTime time, CancellationToken token, Candlestick candlestick, long firstTradeId, long lastTradeId, bool isFinal)
            : base(time, token)
        {
            Throw.IfNull(candlestick, nameof(candlestick));

            if (firstTradeId < 0 && firstTradeId != BinanceApi.NullId)
                throw new ArgumentException($"{nameof(CandlestickEventArgs)}: ID must be greater than 0 (or {nameof(BinanceApi.NullId)}: {BinanceApi.NullId}).", nameof(firstTradeId));
            if (lastTradeId < 0 && lastTradeId != BinanceApi.NullId)
                throw new ArgumentException($"{nameof(CandlestickEventArgs)}: ID must be greater than 0 (or {nameof(BinanceApi.NullId)}: {BinanceApi.NullId}).", nameof(lastTradeId));
            if (lastTradeId < firstTradeId)
                throw new ArgumentException($"{nameof(CandlestickEventArgs)}: Last trade ID must be greater than or equal to first trade ID.", nameof(lastTradeId));

            Candlestick = candlestick;
            FirstTradeId = firstTradeId;
            LastTradeId = lastTradeId;
            IsFinal = isFinal;
        }

        #endregion Constructors
    }
}
