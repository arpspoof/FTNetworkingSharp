using System.Net;

namespace Infra.DataService.Networking
{
    public class DynamicClient : DynamicEndpoint
    {
        public DynamicClient(IPAddress ip, int port) : base(new TCPClient(ip, port)) { }
        public DynamicClient(string webSocketURI) : base(new WsClient(webSocketURI)) { }

        public bool Connect() => client.Connect();
    }
}
