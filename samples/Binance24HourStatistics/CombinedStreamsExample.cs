﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Binance;
using Binance.Application;
using Binance.Market;
using Binance.Utility;
using Binance.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// ReSharper disable AccessToDisposedClosure

namespace Binance24HourStatistics
{
    /// <summary>
    /// Demonstrate how to monitor candlesticks for multiple symbols
    /// and how to unsubscribe/subscribe a symbol after streaming begins.
    /// </summary>
    internal class CombinedStreamsExample
    {
        public static async Task ExampleMain()
        {
            try
            {
                // Load configuration.
                var configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", true, false)
                    .Build();

                // Configure services.
                var services = new ServiceCollection()
                    .AddBinance()
                    .AddLogging(builder => builder.SetMinimumLevel(LogLevel.Trace))
                    .BuildServiceProvider();

                // Configure logging.
                services.GetService<ILoggerFactory>()
                    .AddFile(configuration.GetSection("Logging:File"));

                // Get configuration settings.
                var symbols = configuration.GetSection("CombinedStreamsExample:Symbols").Get<string[]>()
                    ?? new string[] { Symbol.BTC_USDT };

                var client = services.GetService<ISymbolStatisticsWebSocketClient>();

                using (var controller = new RetryTaskController(
                    tkn => client.StreamAsync(tkn),
                    err => Console.WriteLine(err.Message)))
                {
                    if (symbols.Length == 1)
                    {
                        // Subscribe to symbol with callback.
                        client.Subscribe(symbols[0], evt => Display(evt.Statistics[0]));
                    }
                    else
                    {
                        // Alternative usage (combined streams).
                        client.StatisticsUpdate += (s, evt) => { Display(evt.Statistics[0]); };

                        // Subscribe to all symbols.
                        foreach (var symbol in symbols)
                        {
                            client.Subscribe(symbol); // using event instead of callbacks.
                        }
                    }

                    // Begin streaming.
                    controller.Begin();

                    _message = "...press any key to continue.";
                    Console.ReadKey(true); // wait for user input.

                    //*//////////////////////////////////////////////////
                    // Example: Unsubscribe/Subscribe after streaming...

                    // Cancel streaming.
                    await controller.CancelAsync();

                    // Unsubscribe a symbol.
                    client.Unsubscribe(symbols[0]);

                    // Remove unsubscribed symbol and clear display (application specific).
                    _statistics.Remove(symbols[0]);
                    Console.Clear();

                    // Subscribe to the real Bitcoin :D
                    client.Subscribe(Symbol.BCH_USDT); // a.k.a. BCC.

                    // Begin streaming again.
                    controller.Begin();

                    _message = "...press any key to exit.";
                    Console.ReadKey(true); // wait for user input.
                    ///////////////////////////////////////////////////*/
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine();
                Console.WriteLine("  ...press any key to close window.");
                Console.ReadKey(true);
            }
        }

        private static string _message;

        private static readonly object _sync = new object();

        private static readonly IDictionary<string, SymbolStatistics> _statistics
            = new SortedDictionary<string, SymbolStatistics>();

        private static Task _displayTask = Task.CompletedTask;

        private static void Display(SymbolStatistics statsistics)
        {
            lock (_sync)
            {
                _statistics[statsistics.Symbol] = statsistics;

                if (_displayTask.IsCompleted)
                {
                    // Delay to allow multiple data updates between display updates.
                    _displayTask = Task.Delay(100)
                        .ContinueWith(_ =>
                        {
                            SymbolStatistics[] latestStatistics;
                            lock (_sync)
                            {
                                latestStatistics = _statistics.Values.ToArray();
                            }

                            Console.SetCursorPosition(0, 0);

                            foreach (var stats in latestStatistics)
                            {
                                Console.WriteLine($"  24-hour statistics for {stats.Symbol}:");
                                Console.WriteLine($"    %: {stats.PriceChangePercent:0.00} | O: {stats.OpenPrice:0.00000000} | H: {stats.HighPrice:0.00000000} | L: {stats.LowPrice:0.00000000} | V: {stats.Volume:0.}");
                                Console.WriteLine($"    Bid: {stats.BidPrice:0.00000000} | Last: {stats.LastPrice:0.00000000} | Ask: {stats.AskPrice:0.00000000} | Avg: {stats.WeightedAveragePrice:0.00000000}");
                                Console.WriteLine();
                            }

                            Console.WriteLine(_message);
                        });
                }
            }
        }
    }
}
