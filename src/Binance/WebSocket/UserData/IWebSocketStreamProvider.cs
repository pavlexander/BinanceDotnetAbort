﻿namespace Binance.WebSocket.UserData
{
    public interface IWebSocketStreamProvider
    {
        /// <summary>
        /// Create a new <see cref="IWebSocketStream"/>.
        /// </summary>
        /// <returns></returns>
        IWebSocketStream CreateStream();
    }
}
