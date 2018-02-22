﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Binance;
using Binance.Account;
using Binance.Api;

// ReSharper disable PossibleMultipleEnumeration

namespace BinanceConsoleApp.Controllers
{
    internal class GetAccountTrades : IHandleCommand
    {
        public async Task<bool> HandleAsync(string command, CancellationToken token = default)
        {
            if (!command.StartsWith("myTrades ", StringComparison.OrdinalIgnoreCase) &&
                !command.Equals("myTrades", StringComparison.OrdinalIgnoreCase))
                return false;

            if (Program.User == null)
            {
                Program.PrintApiNotice();
                return true;
            }

            var args = command.Split(' ');

            string symbol = Symbol.BTC_USDT;
            var limit = 10;

            if (args.Length > 1)
            {
                if (!int.TryParse(args[1], out limit))
                {
                    symbol = args[1];
                    limit = 10;
                }
            }

            var orderId = BinanceApi.NullId;

            if (args.Length > 2)
            {
                if (symbol.Equals("order", StringComparison.OrdinalIgnoreCase))
                {
                    symbol = Symbol.BTC_USDT;

                    if (!long.TryParse(args[2], out orderId))
                    {
                        symbol = args[2];
                        orderId = BinanceApi.NullId;
                    }
                }
                else
                {
                    if (!int.TryParse(args[2], out limit))
                    {
                        limit = 10;
                    }
                }
            }

            if (args.Length > 3)
            {
                if (!long.TryParse(args[3], out orderId))
                {
                    orderId = BinanceApi.NullId;
                }
            }

            IEnumerable<AccountTrade> trades = null;
            if (orderId >= 0)
            {
                var order = await Program.Api.GetOrderAsync(Program.User, symbol, orderId, token: token);
                if (order != null)
                {
                    trades = await Program.Api.GetTradesAsync(order, token: token);
                }
            }
            else
            {
                trades = await Program.Api.GetAccountTradesAsync(Program.User, symbol, limit: limit, token: token);
            }

            lock (Program.ConsoleSync)
            {
                Console.WriteLine();
                if (trades == null || !trades.Any())
                {
                    Console.WriteLine("[None]");
                }
                else
                {
                    foreach (var trade in trades)
                    {
                        Program.Display(trade);
                    }
                }
                Console.WriteLine();
            }

            return true;
        }
    }
}
