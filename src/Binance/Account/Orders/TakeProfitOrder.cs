﻿using Binance.Api;

namespace Binance.Account.Orders
{
    public sealed class TakeProfitOrder : StopOrder
    {
        #region Public Properties

        public override OrderType Type => OrderType.TakeProfit;

        #endregion Public Properties

        #region Constructors

        public TakeProfitOrder(IBinanceApiUser user)
            : base(user)
        { }

        #endregion Constructors
    }
}
