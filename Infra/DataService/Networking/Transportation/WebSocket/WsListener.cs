using System;
using System.Net;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace Infra.DataService.Networking
{
    public class WsListener
    {
        private HttpListener httpListener = new HttpListener();

        public void Start() => httpListener.Start();
        public void Stop() => httpListener.Stop();

        public WsListener(string prefix) => httpListener.Prefixes.Add(prefix);

        public WebSocket Accept()
        {
            Log("trying to accept a new client");
            WebSocket webSocket = null;
            while (webSocket == null)
            {
                HttpListenerContext listenerContext = httpListener.GetContext();
                Log("get a new client context");
                if (listenerContext.Request.IsWebSocketRequest)
                {
                    WebSocketContext webSocketContext = null;
                    try
                    {
                        Task task = new Task(async () =>
                        {
                            webSocketContext = await listenerContext.AcceptWebSocketAsync(subProtocol: null);
                        });
                        task.Start();
                        task.Wait();
                        webSocket = webSocketContext.WebSocket;
                        Log("success! Accept a new client");
                    }
                    catch
                    {
                        listenerContext.Response.StatusCode = 500;
                        listenerContext.Response.Close();
                        Log("exception throw when trying to accept client");
                    }
                }
                else
                {
                    listenerContext.Response.StatusCode = 400;
                    listenerContext.Response.Close();
                    Log("received non websocket requests");
                }
            }
            return webSocket;
        }

        private void Log(string str)
        {
            Console.WriteLine($"[HTTPListener]: {str}");
        }
    }
}
