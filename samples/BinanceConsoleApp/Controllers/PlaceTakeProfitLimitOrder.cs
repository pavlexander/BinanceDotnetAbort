﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Binance.Account.Orders;

namespace BinanceConsoleApp.Controllers
{
    internal class PlaceTakeProfitLimitOrder : IHandleCommand
    {
        public async Task<bool> HandleAsync(string command, CancellationToken token = default)
        {
            if (!command.StartsWith("takeProfitLimit ", StringComparison.OrdinalIgnoreCase))
                return false;

            var args = command.Split(' ');

            if (args.Length < 6)
            {
                lock (Program.ConsoleSync)
                    Console.WriteLine("A side, symbol, quantity, price, and stop price are required.");
                return true;
            }

            if (!Enum.TryParse(typeof(OrderSide), args[1], true, out var side))
            {
                lock (Program.ConsoleSync)
                    Console.WriteLine("A valid order side is required ('buy' or 'sell').");
                return true;
            }

            var symbol = args[2];

            if (!decimal.TryParse(args[3], out var quantity) || quantity <= 0)
            {
                lock (Program.ConsoleSync)
                    Console.WriteLine("A quantity greater than 0 is required.");
                return true;
            }

            if (!decimal.TryParse(args[4], out var price) || price <= 0)
            {
                lock (Program.ConsoleSync)
                    Console.WriteLine("A price greater than 0 is required.");
                return true;
            }

            if (!decimal.TryParse(args[5], out var stopPrice) || stopPrice <= 0)
            {
                lock (Program.ConsoleSync)
                    Console.WriteLine("A stop price greater than 0 is required.");
                return true;
            }

            var clientOrder = new TakeProfitLimitOrder(Program.User)
            {
                Symbol = symbol,
                Side = (OrderSide)side,
                Quantity = quantity,
                Price = price,
                StopPrice = stopPrice
            };

            if (Program.IsOrdersTestOnly)
            {
                await Program.Api.TestPlaceAsync(clientOrder, token: token);

                lock (Program.ConsoleSync)
                {
                    Console.WriteLine($"~ TEST ~ >> TAKE PROFIT LIMIT {clientOrder.Side} order (ID: {clientOrder.Id}) placed for {clientOrder.Quantity:0.00000000} {clientOrder.Symbol} @ {clientOrder.Price:0.00000000}");
                }
            }
            else
            {
                var order = await Program.Api.PlaceAsync(clientOrder, token: token);

                // ReSharper disable once InvertIf
                if (order != null)
                {
                    lock (Program.ConsoleSync)
                    {
                        Console.WriteLine($">> TAKE PROFIT LIMIT {order.Side} order (ID: {order.Id}) placed for {order.OriginalQuantity:0.00000000} {order.Symbol} @ {order.Price:0.00000000}");
                    }
                }
            }

            return true;
        }
    }
}
