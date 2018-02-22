﻿using System;
using System.Threading;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace Binance.Api
{
    public interface IApiRateLimiter
    {
        #region Properties

        /// <summary>
        /// Get or set the rate limiting enabled flag.
        /// </summary>
        bool IsEnabled { get; set; }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Set a duration and maximum count (remove with count = 0).
        /// Rate is limited to count/duration.
        /// </summary>
        /// <param name="duration"></param>
        /// <param name="count"></param>
        void Configure(TimeSpan duration, int count);

        /// <summary>
        /// Perform the rate limiting delay.
        /// </summary>
        /// <param name="count"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task DelayAsync(int count = 1, CancellationToken token = default);

        #endregion Methods
    }
}
