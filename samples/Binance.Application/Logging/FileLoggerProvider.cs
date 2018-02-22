﻿using Microsoft.Extensions.Logging;

namespace Binance.Application.Logging
{
    public sealed class FileLoggerProvider : ILoggerProvider
    {
        private readonly ILogger _fileLogger;

        public FileLoggerProvider(string filePath, LogLevel level)
        {
            _fileLogger = new FileLogger(filePath, level);
        }

        public ILogger CreateLogger(string categoryName)
        {
            return _fileLogger;
        }

        public void Dispose()
        { }
    }
}
