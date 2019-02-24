using System;
using System.IO;
using System.Net.WebSockets;
using System.Threading;

namespace Infra.DataService.Networking
{
    public abstract class WsEndpoint : ITransportationLayer
    {
        public WebSocket WsClient { get; protected set; }

        public abstract event Action ConnectionEstablished;

        public event Action ConnectionLost;
        public event Action<Stream> ProviderDataReady;
        
        private const int bufferSize = 102400;
        private readonly byte[] receiveBuffer = new byte[bufferSize];
        private readonly byte[] sendBuffer = new byte[bufferSize];
        
        private bool online = false;
        private CancellationTokenSource cancellationTokenSource;

        protected void Init()
        {
            online = true;
            cancellationTokenSource = new CancellationTokenSource();
            BeginReadAsync();
        }

        public void Encapsulate(Stream data)
        {
            if (!online) return;
            int nRead = data.Read(sendBuffer, 0, bufferSize);
            try
            {
                Log($"trying to send data");
                WsClient.SendAsync(new ArraySegment<byte>(sendBuffer, 0, nRead),
                    WebSocketMessageType.Binary, false, cancellationTokenSource.Token).Wait();
                Log($"trying to send data, done");
            }
            catch (Exception e)
            {
                Log($"exception thrown when sending new data {e.Message}");
                CloseAndRaise();
                return;
            }
            if (nRead < data.Length)
            {
                Log($"data is too large {data.Length}, sending rest of it");
                Encapsulate(data);
            }
        }

        private void BeginReadAsync()
        {
            var token = cancellationTokenSource.Token;
            async void StartRead ()
            {
                while (online)
                {
                    WebSocketReceiveResult result = null;
                    try
                    {
                        Log("awaiting new data");
                        result = await WsClient.ReceiveAsync(new ArraySegment<byte>(receiveBuffer), cancellationTokenSource.Token);
                        Log("new data coming");
                    }
                    catch (Exception e)
                    {
                        Log($"exception thrown when awaiting new data {e.Message}");
                        CloseAndRaise();
                        return;
                    }
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        Log($"message type is [Close]");
                        CloseAndRaise();
                    }
                    else
                    {
                        using (MemoryStream stream = new MemoryStream())
                        {
                            stream.Write(receiveBuffer, 0, result.Count);
                            stream.Seek(0, SeekOrigin.Begin);
                            Log($"passing data to upstream processor");
                            ProviderDataReady?.Invoke(stream);
                            Log($"passing data to upstream processor, done");
                        }
                    }
                }
                Log($"read thread exiting, since it is not connected");
            }
            StartRead();
        }

        public void CloseConnection()
        {
            if (online && WsClient != null)
            {
                online = false;
                Log("cancelling tasks");
                cancellationTokenSource.Cancel();
                Log("starting to close connection");
                WsClient.Abort();
                WsClient.Dispose();
                Log("finish to close connection");
            }
        }

        private void CloseAndRaise()
        {
            CloseConnection();
            Log("invoking connection lost");
            ConnectionLost?.Invoke();
            Log("invoking connection lost, done");
        }

        protected void Log(string str)
        {
            Logger.Log(str, "Ws");
        }
    }
}
