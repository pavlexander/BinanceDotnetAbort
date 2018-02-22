﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Binance;
using Binance.Market;

namespace BinanceConsoleApp.Controllers
{
    internal class GetCandlesticks : IHandleCommand
    {
        public async Task<bool> HandleAsync(string command, CancellationToken token = default)
        {
            if (!command.StartsWith("candles ", StringComparison.OrdinalIgnoreCase) &&
                !command.Equals("candles", StringComparison.OrdinalIgnoreCase) &&
                !command.StartsWith("kLines ", StringComparison.OrdinalIgnoreCase) &&
                !command.Equals("kLines", StringComparison.OrdinalIgnoreCase))
                return false;

            var args = command.Split(' ');

            string symbol = Symbol.BTC_USDT;
            if (args.Length > 1)
            {
                symbol = args[1];
            }

            var interval = CandlestickInterval.Hour;
            if (args.Length > 2)
            {
                interval = args[2].ToCandlestickInterval();
            }

            var limit = 10;
            if (args.Length > 3)
            {
                int.TryParse(args[3], out limit);
            }

            IEnumerable<Candlestick> candlesticks = null;

            // TODO: If live candlestick cache is active (for symbol), get cached data.
            //if (Program.CandlestickCache != null && Program.CandlestickCache.Candlesticks.FirstOrDefault()?.Symbol == symbol)
            //    candlesticks = Program.CandlestickCache.Candlesticks.Reverse().Take(limit); // get local cache.

            if (candlesticks == null)
                candlesticks = await Program.Api.GetCandlesticksAsync(symbol, interval, limit, token: token);

            lock (Program.ConsoleSync)
            {
                Console.WriteLine();
                foreach (var candlestick in candlesticks)
                {
                    Program.Display(candlestick);
                }
                Console.WriteLine();
            }

            return true;
        }
    }
}
