﻿using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Logging;
using System;

namespace MediaBrowser.ApiInteraction.WebSocket
{
    /// <summary>
    /// Class ClientWebSocketFactory
    /// </summary>
    public static class ClientWebSocketFactory
    {
        /// <summary>
        /// Creates the web socket.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <returns>IClientWebSocket.</returns>
        public static IClientWebSocket CreateWebSocket(ILogger logger)
        {
            try
            {
                // This is preferred but only supported on windows 8 or server 2012
                // Comment NativeClientWebSocket out for now due to message parsing errors
                // return new NativeClientWebSocket(logger);
                return new WebSocket4NetClientWebSocket(logger);
            }
            catch (NotSupportedException)
            {
                return new WebSocket4NetClientWebSocket(logger);
            }
        }

        /// <summary>
        /// Creates the web socket.
        /// </summary>
        /// <returns>IClientWebSocket.</returns>
        public static IClientWebSocket CreateWebSocket()
        {
            return CreateWebSocket(new NullLogger());
        }
    }

    public static class SocketExtensions
    {
        public static void OpenWebSocket(this ApiClient client)
        {
            client.OpenWebSocket(() => ClientWebSocketFactory.CreateWebSocket(new NullLogger()));
        }
    }
}
