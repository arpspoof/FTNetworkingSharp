using System;
using System.Net.WebSockets;

namespace Infra.DataService.Networking
{
    public class WsRemoteClient : WsEndpoint
    {
        public override event Action ConnectionEstablished;

        public void Bind(WebSocket wsClient)
        {
            WsClient?.Dispose();
            WsClient = wsClient;
            Init();
            ConnectionEstablished?.Invoke();
        }
    }
}
