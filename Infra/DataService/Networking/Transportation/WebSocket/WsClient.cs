using System;
using System.Net.WebSockets;
using System.Threading;

namespace Infra.DataService.Networking
{
    public class WsClient : WsEndpoint
    {
        public override event Action ConnectionEstablished;
        
        public string URI { get; }

        public WsClient(string uri) => URI = uri;

        public bool Connect()
        {
            Log($"trying to connect to {URI}, {Thread.CurrentThread.ManagedThreadId}");
            try
            {
                CloseConnection();
                var client = new ClientWebSocket();
                WsClient = client;
                client.ConnectAsync(new Uri(URI), CancellationToken.None).Wait();
                Log($"success! connect to {URI}");
                Init();
                Log($"start to read data");
                ConnectionEstablished?.Invoke();
                Log($"notify that connection is established");
                return true;
            }
            catch (Exception e)
            {
                Log($"exception thrown when connecting {e.Message}");
                WsClient?.Dispose();
                return false;
            }
        }
    }
}
