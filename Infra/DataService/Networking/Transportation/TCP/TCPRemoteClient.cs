using System;
using System.Net.Sockets;

namespace Infra.DataService.Networking
{
    public class TCPRemoteClient : TCPEndpoint
    {
        public override event Action ConnectionEstablished;

        public void Bind(TcpClient tcpClient)
        {
            TCPClient?.Dispose();
            TCPClient = tcpClient;
            DataStream = tcpClient.GetStream();
            Init();
            ConnectionEstablished?.Invoke();
        }
    }
}
