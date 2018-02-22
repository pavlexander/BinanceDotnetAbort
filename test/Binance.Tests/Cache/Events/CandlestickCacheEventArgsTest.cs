﻿using Binance.Cache.Events;
using Binance.Market;
using System;
using Xunit;

namespace Binance.Tests.Cache.Events
{
    public class CandlestickCacheEventArgsTest
    {
        [Fact]
        public void Throws()
        {
            Assert.Throws<ArgumentNullException>("candlesticks", () => new CandlestickCacheEventArgs(null));
        }

        [Fact]
        public void Properties()
        {
            var symbol = Symbol.BTC_USDT;
            const CandlestickInterval interval = CandlestickInterval.Hour;
            var openTime = DateTime.UtcNow;
            const decimal open = 4950;
            const decimal high = 5100;
            const decimal low = 4900;
            const decimal close = 5050;
            const decimal volume = 1000;
            var closeTime = openTime.AddHours(1);
            const long quoteAssetVolume = 5000000;
            const int numberOfTrades = 555555;
            const decimal takerBuyBaseAssetVolume = 4444;
            const decimal takerBuyQuoteAssetVolume = 333;

            var candlestick = new Candlestick(symbol, interval, openTime, open, high, low, close, volume, closeTime, quoteAssetVolume, numberOfTrades, takerBuyBaseAssetVolume, takerBuyQuoteAssetVolume);

            var candlesticks = new[] { candlestick };

            var args = new CandlestickCacheEventArgs(candlesticks);

            Assert.Equal(candlesticks, args.Candlesticks);
        }
    }
}
