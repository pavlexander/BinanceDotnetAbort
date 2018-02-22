﻿using System.Threading;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace Binance.WebSocket
{
    public static class AggregateTradeWebSocketClientExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="client"></param>
        /// <param name="symbol"></param>
        public static void Subscribe(this IAggregateTradeWebSocketClient client, string symbol)
            => client.Subscribe(symbol, null);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="client"></param>
        /// <param name="symbol"></param>
        public static void Unsubscribe(this IAggregateTradeWebSocketClient client, string symbol)
            => client.Unsubscribe(symbol, null);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="client"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static Task StreamAsync(this IAggregateTradeWebSocketClient client, CancellationToken token)
        {
            Throw.IfNull(client, nameof(client));

            return client.WebSocket.StreamAsync(token);
        }
    }
}
