using System;
using System.Net;
using System.Net.Sockets;

namespace Infra.DataService.Networking
{
    public class TCPClient : TCPEndpoint
    {
        public override event Action ConnectionEstablished;

        public IPAddress IPAddress { get; }
        public int Port { get; }

        public TCPClient(IPAddress ip, int port)
        {
            IPAddress = ip;
            Port = port;
        }

        public bool Connect()
        {
            CloseConnection();
            TCPClient = new TcpClient();
            try
            {
                TCPClient.Connect(IPAddress, Port);
                DataStream = TCPClient.GetStream();
                Log($"connect successully!!");
                Init();
                Log($"invoking ConnectionEstablished");
                ConnectionEstablished?.Invoke();
                Log($"invoking ConnectionEstablished, done");
                return true;
            }
            catch (Exception e)
            {
                Log($"exception thrown when connecting {e.Message}");
                TCPClient.Dispose();
                return false;
            }
        }
    }
}
