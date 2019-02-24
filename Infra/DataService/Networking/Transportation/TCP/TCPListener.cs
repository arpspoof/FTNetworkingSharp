using System.Net;
using System.Net.Sockets;

namespace Infra.DataService.Networking
{
    public class TCPListener
    {
        private readonly TcpListener listener;

        public void Start() => listener.Start();
        public void Stop() => listener.Stop();

        public TCPListener(IPAddress localIP, int port) 
            => listener = new TcpListener(localIP, port);

        public TcpClient Accept() => listener.AcceptTcpClient();
    }
}
