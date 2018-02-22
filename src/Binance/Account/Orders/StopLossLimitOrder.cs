﻿using Binance.Api;

namespace Binance.Account.Orders
{
    public sealed class StopLossLimitOrder : StopLimitOrder
    {
        #region Public Properties

        public override OrderType Type => OrderType.StopLossLimit;

        #endregion Public Properties

        #region Constructors

        public StopLossLimitOrder(IBinanceApiUser user)
            : base(user)
        { }

        #endregion Constructors
    }
}
