using System.Net;

namespace Infra.DataService.Networking
{
    public class DynamicListener
    {
        private readonly dynamic listener;

        public DynamicListener(IPAddress localIP, int port)
            => listener = new TCPListener(localIP, port);

        public DynamicListener(string webSocketURI)
            => listener = new WsListener(webSocketURI);

        public void Start() => listener.Start();
        public void Stop() => listener.Stop();

        public DynamicRemoteClient Accept()
        {
            dynamic client = listener.Accept();
            dynamic remote = GetRemoteClient();
            return new DynamicRemoteClient(remote, client);
        }

        private dynamic GetRemoteClient()
        {
            switch (listener)
            {
                case TCPListener listener: return new TCPRemoteClient();
                case WsListener listener: return new WsRemoteClient();
                default: return null;
            }
        }
    }
}
