using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Infra.DataService.Networking;
using Infra.DataService.Protocol;
using TestData;

namespace TestFileTransferServer
{
    class TestFileTransferServer
    {
        static void Main(string[] args)
        {
            string filePath = "E:/test.zip";
            FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);

            DynamicListener listener = new DynamicListener(/*IPAddress.Any*/IPAddress.Parse("192.168.1.73"), 54321);
            // netsh http add urlacl url=http://+:54322/ws user=everyone
            //WsListener listener = new WsListener("http://+:54322/ws/");
            listener.Start();

            //TCPRemoteClient client = new TCPRemoteClient();
            //WsRemoteClient client = new WsRemoteClient();

            ProtocolTree tree = new ProtocolTree();
            LeafProtocolHandler<Data> leaf = new LeafProtocolHandler<Data>();
            LeafProtocolHandler<Rec> rec = new LeafProtocolHandler<Rec>();
            LeafProtocolHandler<Done> done = new LeafProtocolHandler<Done>();
            tree.Register(leaf);
            tree.Register(rec);
            tree.Register(done);
            tree.EntryToLeaf(leaf);
            tree.EntryToLeaf(rec);
            tree.EntryToLeaf(done);

            ApplicationConnectionManager app = null;

            //TcpClient cl = null;
            //WebSocket cl = null;

            bool finish = false;
            Task task = null;

            bool connected = false;

            void clean()
            {
                Console.WriteLine("conn lost + ");
                connected = false;
                Console.WriteLine("connection is down");
                reconn();
            }

            void rd(Rec data)
            {
                task = new Task(() =>
                {
                    Console.WriteLine("sen task started");
                    long p = data.seq;
                    while (p < stream.Length)
                    {
                        if (!connected) { Console.WriteLine("sen task ended"); return; }
                        long blockSize = Math.Min(65536, stream.Length - p);
                        stream.Seek(p, SeekOrigin.Begin);
                        byte[] b = new byte[blockSize];
                        stream.Read(b, 0, (int)blockSize);
                        p += blockSize;
                        Data d = new Data(b);
                        Console.WriteLine("sen task send");
                        leaf.Send(d);
                        Console.WriteLine("sen task send, done");
                        Console.WriteLine($"{blockSize} bytes sent, total {p} sent");
                        Thread.Sleep(15);
                    }
                    Console.WriteLine("sen task fined");
                    done.Send(new Done());
                });
                task.Start();
            }

            void fin(Done d) => finish = true;

            // client.ConnectionLost += clean;
            rec.NewData += rd;
            done.NewData += fin;

            void reconn()
            {
                while (true)
                {
                    if (connected) return;
                    try
                    {
                        Console.WriteLine("waiting...");
                        DynamicRemoteClient cl = listener.Accept();
                        if (app != null) app.Dispose();
                        app = new ApplicationConnectionManager(cl, tree, 1000, 4000);
                        app.ConnectionLost += clean;
                        cl.Activate();
                        //cl = listener.AcceptWsClient();
                        connected = true;
                        Console.WriteLine("accepted! "/* + cl.Client.RemoteEndPoint.ToString()*/);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("network error");
                        Thread.Sleep(1000);
                        continue;
                    }
                }
            }

            reconn();

            while (true)
            {
                if (finish)
                {
                    Console.WriteLine("finished!!");
                    break;
                }
                else Thread.Sleep(500);
            }
        }
    }
}
