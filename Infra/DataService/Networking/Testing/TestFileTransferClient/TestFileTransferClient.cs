using System;
using System.IO;
using System.Net;
using System.Threading;
using Infra.DataService.Networking;
using Infra.DataService.Protocol;
using TestData;

namespace TestFileTransferClient
{
    public class ST : IStateDataProvider
    {
        public long seq;
        public AbstractProtocol GetStateRecoveryData()
        {
            Console.WriteLine("sending rec data ...");
            var a = new Rec();
            a.seq = seq;
            return a;
        }
    }

    class TestFileTransferClient
    {
        static void Main(string[] args)
        {
            string filePath = "E:/r.txt";

            if (!File.Exists(filePath))
            {
                Console.WriteLine("transfer complete");
                return;
            }

            StreamReader r = new StreamReader(filePath);

            ST state = new ST();
            state.seq = Convert.ToInt64(r.ReadLine());

            r.Close();

            DynamicClient client = new DynamicClient(IPAddress.Parse("192.168.1.73"), 54321);
            //WsClient client = new WsClient("ws://192.168.1.73:54322/ws");

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

            ApplicationConnectionManager app = new ApplicationConnectionManager(client, tree, state, 1000, 2000);

            bool finish = false;

            void conn()
            {
                while (true)
                {
                    Console.WriteLine("connecting... ");
                    if (client.Connect())
                    {
                        Console.WriteLine("connected!!!! ");
                        break;
                    }
                    else
                    {
                        Console.WriteLine("connect failed");
                        Thread.Sleep(1000);
                    }
                }
            }

            /*client.ConnectionLost += () =>
            {
                conn();
            };*/

            app.ConnectionLost += () =>
            {
                conn();
            };

            string wpath = "E:/recv.zip";
            FileStream ws = new FileStream(wpath, FileMode.Append, FileAccess.Write);
            ws.Seek(state.seq, SeekOrigin.Begin);

            leaf.NewData += d =>
            {
                try
                {
                    ws.Write(d.data, 0, d.data.Length);
                    ws.Flush();
                    state.seq += d.data.Length;
                    File.WriteAllText(filePath, state.seq.ToString());
                    Console.WriteLine($"received {d.data.Length}, now pointer is {state.seq} ");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            };

            done.NewData += obj =>
            {
                finish = true;
                File.Delete(filePath);
            };

            conn();

            while (true)
            {
                if (finish)
                {
                    Console.WriteLine("finished!!");
                    break;
                }
                Thread.Sleep(100);
            }
        }
    }
}
