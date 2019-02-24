using System.Net.Sockets;
using System.Net.WebSockets;

namespace Infra.DataService.Networking
{
    public class DynamicRemoteClient : DynamicEndpoint
    {
        private readonly dynamic remoteEndpoint;
        private readonly dynamic actualClient;

        internal DynamicRemoteClient(TCPRemoteClient remote, TcpClient client) : base(remote)
        {
            remoteEndpoint = remote;
            actualClient = client;
        }

        internal DynamicRemoteClient(WsRemoteClient remote, WebSocket client) : base(remote)
        {
            remoteEndpoint = remote;
            actualClient = client;
        }

        public void Activate()
        {
            remoteEndpoint.Bind(actualClient);
        }
    }
}
