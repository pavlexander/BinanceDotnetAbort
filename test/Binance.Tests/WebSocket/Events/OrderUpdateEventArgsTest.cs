﻿using System;
using System.Threading;
using Binance.Account.Orders;
using Binance.Api;
using Binance.WebSocket.Events;
using Xunit;

namespace Binance.Tests.WebSocket.Events
{
    public class OrderUpdateEventArgsTest
    {
        [Fact]
        public void Throws()
        {
            var time = DateTimeOffset.FromUnixTimeMilliseconds(DateTime.UtcNow.ToTimestamp()).UtcDateTime;

            const OrderExecutionType orderExecutionType = OrderExecutionType.New;
            const OrderRejectedReason orderRejectedReason = OrderRejectedReason.None;
            const string newClientOrderId = "new-test-order";

            using (var cts = new CancellationTokenSource())
            {
                Assert.Throws<ArgumentNullException>("order", () => new OrderUpdateEventArgs(time, cts.Token, null, orderExecutionType, orderRejectedReason, newClientOrderId));
            }
        }

        [Fact]
        public void Properties()
        {
            var time = DateTimeOffset.FromUnixTimeMilliseconds(DateTime.UtcNow.ToTimestamp()).UtcDateTime;

            var user = new BinanceApiUser("api-key");
            var symbol = Symbol.BTC_USDT;
            const long id = 123456;
            const string clientOrderId = "test-order";
            const decimal price = 4999;
            const decimal originalQuantity = 1;
            const decimal executedQuantity = 0.5m;
            const OrderStatus status = OrderStatus.PartiallyFilled;
            const TimeInForce timeInForce = TimeInForce.IOC;
            const OrderType orderType = OrderType.Market;
            const OrderSide orderSide = OrderSide.Sell;
            const decimal stopPrice = 5000;
            const decimal icebergQuantity = 0.1m;
            const bool isWorking = true;

            var order = new Order(user, symbol, id, clientOrderId, price, originalQuantity, executedQuantity, status, timeInForce, orderType, orderSide, stopPrice, icebergQuantity, time, isWorking);

            const OrderExecutionType orderExecutionType = OrderExecutionType.New;
            const OrderRejectedReason orderRejectedReason = OrderRejectedReason.None;
            const string newClientOrderId = "new-test-order";

            using (var cts = new CancellationTokenSource())
            {
                var args = new OrderUpdateEventArgs(time, cts.Token, order, orderExecutionType, orderRejectedReason, newClientOrderId);

                Assert.Equal(time, args.Time);
                Assert.Equal(order, args.Order);
                Assert.Equal(orderExecutionType, args.OrderExecutionType);
                Assert.Equal(orderRejectedReason, args.OrderRejectedReason);
                Assert.Equal(newClientOrderId, args.NewClientOrderId);
            }
        }
    }
}
