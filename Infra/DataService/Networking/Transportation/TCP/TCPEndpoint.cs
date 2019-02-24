using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;

namespace Infra.DataService.Networking
{
    public abstract class TCPEndpoint : ITransportationLayer
    {
        public TcpClient TCPClient { get; protected set; }
        public NetworkStream DataStream { get; protected set; }

        public abstract event Action ConnectionEstablished;

        public event Action ConnectionLost;
        public event Action<Stream> ProviderDataReady;
        
        private const int bufferSize = 102400;
        private readonly byte[] receiveBuffer = new byte[bufferSize];

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
            try
            {
                Log($"sending data of {data.Length} bytes");
                data.CopyTo(DataStream);
                DataStream.Flush();
                Log($"sending data, done");
            }
            catch (Exception e)
            {
                Log($"exception thrown when sending data {e.Message}");
                CloseAndRaise();
            }
        }

        protected void BeginReadAsync()
        {
            var token = cancellationTokenSource.Token;
            async void StartRead()
            {
                while (online)
                {
                    int nRead = 0;
                    try
                    {
                        Log("awaiting new data");
                        nRead = await DataStream.ReadAsync(receiveBuffer, 0, bufferSize, cancellationTokenSource.Token);
                        Log("new data coming");
                    }
                    catch (Exception e)
                    {
                        Log($"exception thrown when awaiting new data {e.Message}");
                        CloseAndRaise();
                        return;
                    }
                    if (nRead == 0)
                    {
                        Log($"read 0 bytes");
                        CloseAndRaise();
                    }
                    else
                    {
                        using (MemoryStream stream = new MemoryStream())
                        {
                            stream.Write(receiveBuffer, 0, nRead);
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
            Log($"calling close connection");
            if (online)
            {
                online = false;
                Log("cancelling tasks");
                cancellationTokenSource.Cancel();
                Log("closing tcp connection");
                TCPClient?.Close();
                TCPClient?.Dispose();
                Log("closing tcp connection, done");
            }
            Log($"calling close connection, done");
        }

        private void CloseAndRaise()
        {
            CloseConnection();
            Log($"invoking ConnectionLost");
            ConnectionLost?.Invoke();
            Log($"invoking ConnectionLost, done");
        }

        protected void Log(string str)
        {
            Logger.Log(str, "TCP");
        }
    }
}
