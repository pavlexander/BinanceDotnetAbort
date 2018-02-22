﻿using System;
using System.Collections.Generic;
using System.Threading;
using Binance.Market;
using Binance.WebSocket.Events;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace Binance.WebSocket
{
    /// <summary>
    /// An <see cref="IAggregateTradeWebSocketClient"/> implementation.
    /// </summary>
    public class AggregateTradeWebSocketClient : BinanceWebSocketClient<AggregateTradeEventArgs>, IAggregateTradeWebSocketClient
    {
        #region Public Events

        public event EventHandler<AggregateTradeEventArgs> AggregateTrade;

        #endregion Public Events

        #region Constructors

        /// <summary>
        /// Default constructor provides default web socket stream, but no logging.
        /// </summary>
        public AggregateTradeWebSocketClient()
            : this(new BinanceWebSocketStream())
        { }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="logger"></param>
        public AggregateTradeWebSocketClient(IWebSocketStream stream, ILogger<AggregateTradeWebSocketClient> logger = null)
            : base(stream, logger)
        { }

        #endregion Construtors

        #region Public Methods

        public virtual void Subscribe(string symbol, Action<AggregateTradeEventArgs> callback)
        {
            Throw.IfNullOrWhiteSpace(symbol, nameof(symbol));

            symbol = symbol.FormatSymbol();

            Logger?.LogDebug($"{nameof(AggregateTradeWebSocketClient)}.{nameof(Subscribe)}: \"{symbol}\" (callback: {(callback == null ? "no" : "yes")}).  [thread: {Thread.CurrentThread.ManagedThreadId}]");

            SubscribeStream(GetStreamName(symbol), callback);
        }

        public virtual void Unsubscribe(string symbol, Action<AggregateTradeEventArgs> callback)
        {
            Throw.IfNullOrWhiteSpace(symbol, nameof(symbol));

            symbol = symbol.FormatSymbol();

            Logger?.LogDebug($"{nameof(AggregateTradeWebSocketClient)}.{nameof(Unsubscribe)}: \"{symbol}\" (callback: {(callback == null ? "no" : "yes")}).  [thread: {Thread.CurrentThread.ManagedThreadId}]");

            UnsubscribeStream(GetStreamName(symbol), callback);
        }

        #endregion Public Methods

        #region Protected Methods

        protected override void OnWebSocketEvent(WebSocketStreamEventArgs args, IEnumerable<Action<AggregateTradeEventArgs>> callbacks)
        {
            Logger?.LogDebug($"{nameof(AggregateTradeWebSocketClient)}: \"{args.Json}\"  [thread: {Thread.CurrentThread.ManagedThreadId}]");

            try
            {
                var jObject = JObject.Parse(args.Json);

                var eventType = jObject["e"].Value<string>();

                if (eventType == "aggTrade")
                {
                    var eventTime = jObject["E"].Value<long>().ToDateTime();

                    var trade = new AggregateTrade(
                        jObject["s"].Value<string>(),  // symbol
                        jObject["a"].Value<long>(),    // aggregate trade ID
                        jObject["p"].Value<decimal>(), // price
                        jObject["q"].Value<decimal>(), // quantity
                        jObject["f"].Value<long>(),    // first trade ID
                        jObject["l"].Value<long>(),    // last trade ID
                        jObject["T"].Value<long>()     // trade time
                            .ToDateTime(),
                        jObject["m"].Value<bool>(),    // is buyer the market maker?
                        jObject["M"].Value<bool>());   // is best price match?

                    var eventArgs = new AggregateTradeEventArgs(eventTime, args.Token, trade);

                    try
                    {
                        if (callbacks != null)
                        {
                            foreach (var callback in callbacks)
                                callback(eventArgs);
                        }
                        AggregateTrade?.Invoke(this, eventArgs);
                    }
                    catch (OperationCanceledException) { }
                    catch (Exception e)
                    {
                        if (!args.Token.IsCancellationRequested)
                        {
                            Logger?.LogError(e, $"{nameof(AggregateTradeWebSocketClient)}: Unhandled aggregate trade event handler exception.  [thread: {Thread.CurrentThread.ManagedThreadId}]");
                        }
                    }
                }
                else
                {
                    Logger?.LogWarning($"{nameof(AggregateTradeWebSocketClient)}.{nameof(OnWebSocketEvent)}: Unexpected event type ({eventType}).  [thread: {Thread.CurrentThread.ManagedThreadId}]");
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception e)
            {
                if (!args.Token.IsCancellationRequested)
                {
                    Logger?.LogError(e, $"{nameof(AggregateTradeWebSocketClient)}.{nameof(OnWebSocketEvent)}: Failed.  [thread: {Thread.CurrentThread.ManagedThreadId}]");
                }
            }
        }

        #endregion Protected Methods

        #region Private Methods

        private static string GetStreamName(string symbol)
            => $"{symbol.ToLowerInvariant()}@aggTrade";

        #endregion Private Methods
    }
}
