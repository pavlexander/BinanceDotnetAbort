﻿using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Binance;
using Binance.Api;
using Binance.Application;
using Binance.Cache;
using Binance.Market;
using Binance.Utility;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// ReSharper disable AccessToDisposedClosure

namespace BinanceMarketDepth
{
    /// <summary>
    /// Demonstrate how to maintain multiple order book caches
    /// and respond to real-time depth-of-market update events.
    /// </summary>
    internal class MultiCacheExample
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

                Console.Clear(); // clear the display.

                var limit = 5;

                var api = services.GetService<IBinanceApi>();

                // Query and display the order books.
                var btcOrderBook = await api.GetOrderBookAsync(Symbol.BTC_USDT, limit);
                var ethOrderBook = await api.GetOrderBookAsync(Symbol.ETH_BTC, limit);
                Display(btcOrderBook, ethOrderBook);

                var btcCache = services.GetService<IOrderBookCache>();
                var ethCache = services.GetService<IOrderBookCache>();

                Func<CancellationToken, Task> action1 =
                    tkn => btcCache.SubscribeAndStreamAsync(Symbol.BTC_USDT, limit,
                        evt =>
                        {
                            btcOrderBook = evt.OrderBook;
                            Display(btcOrderBook, ethOrderBook);
                        }, tkn);

                Func<CancellationToken, Task> action2 =
                    tkn => ethCache.SubscribeAndStreamAsync(Symbol.ETH_BTC, limit,
                        evt =>
                        {
                            ethOrderBook = evt.OrderBook;
                            Display(btcOrderBook, ethOrderBook);
                        }, tkn);

                Action<Exception> onError = err => Console.WriteLine(err.Message);

                using (var controller1 = new RetryTaskController(action1, onError))
                using (var controller2 = new RetryTaskController(action2, onError))
                {
                    //////////////////////////////////////////////////////////////////////
                    // Using multiple controllers and multiple (non-combined) web sockets.
                    // NOTE: IWebSocketStream must be left as Transient in DI setup.

                    // Monitor order book and display updates in real-time.
                    controller1.Begin();

                    // Monitor order book and display updates in real-time.
                    controller2.Begin();

                    // Verify we are not using a shared/combined stream (not necessary).
                    if (btcCache.Client.WebSocket.IsCombined || ethCache.Client.WebSocket.IsCombined || btcCache.Client.WebSocket == ethCache.Client.WebSocket)
                        throw new Exception(":(");

                    Console.ReadKey(true);
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

        private static readonly object _displaySync = new object();

        private static void Display(params OrderBook[] orderBooks)
        {
            lock (_displaySync)
            {
                Console.SetCursorPosition(0, 0);

                foreach (var orderBook in orderBooks)
                {
                    orderBook.Print(Console.Out);
                    Console.WriteLine();
                }

                Console.WriteLine("...press any key to exit.");
            }
        }
    }
}
