﻿using Binance.Cache.Events;
using Binance.Market;
using System;
using Xunit;

namespace Binance.Tests.Cache.Events
{
    public class AggregateTradeCacheEventArgsTest
    {
        [Fact]
        public void Throws()
        {
            Assert.Throws<ArgumentNullException>("trades", () => new AggregateTradeCacheEventArgs(null));
        }

        [Fact]
        public void Properties()
        {
            var time = DateTimeOffset.FromUnixTimeMilliseconds(DateTime.UtcNow.ToTimestamp()).UtcDateTime;

            var symbol = Symbol.BTC_USDT;
            const long id = 12345;
            const decimal price = 5000;
            const decimal quantity = 1;
            const long firstTradeId = 123456;
            const long lastTradeId = 234567;
            const bool isBuyerMaker = true;
            const bool isBestPriceMatch = true;

            var trade = new AggregateTrade(symbol, id, price, quantity, firstTradeId, lastTradeId, time, isBuyerMaker, isBestPriceMatch);

            var trades = new[] { trade };

            var args = new AggregateTradeCacheEventArgs(trades);

            Assert.Equal(trades, args.Trades);
        }
    }
}
