﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Binance.Api;

namespace BinanceConsoleApp.Controllers
{
    internal class GetTime : IHandleCommand
    {
        public async Task<bool> HandleAsync(string command, CancellationToken token = default)
        {
            if (!command.Equals("time", StringComparison.OrdinalIgnoreCase))
                return false;

            var time = await Program.Api.GetTimeAsync(token);
            var timestamp = await Program.Api.GetTimestampAsync(token);

            lock (Program.ConsoleSync)
            {
                Console.WriteLine($"  {time.Kind.ToString().ToUpperInvariant()} Time: {time}  [Local: {time.ToLocalTime()}]  Timestamp: {timestamp} [offset: {Program.Api.HttpClient.TimestampProvider?.TimestampOffset ?? 0}]");
                Console.WriteLine();
            }

            return true;
        }
    }
}
