using System;
using System.Net;
using System.Threading;
using Infra.ServiceFramework.Client;
using NumGame.Logic;

namespace NumGame.Client
{
    class Client
    {
        static void Main(string[] args)
        {
            Infra.Logger.Filter.Add("TCP");
            Infra.Logger.Filter.Add("DS");
            Infra.Logger.Filter.Add("FT");
            bool reject = false;
            Console.WriteLine("interface:");
            int id = Convert.ToInt32(Console.ReadLine().Trim());
            if (id < 0 || id > 1) return;
            Console.WriteLine("token:");
            long token = Convert.ToInt64(Console.ReadLine().Trim());

            IClientLogic logic = new ClientLogic(id);
            ServiceClient client = new ServiceClient(logic, IPAddress.Parse("192.168.1.73"), 54321, id, id);
            client.ResumeToken = token;
            client.ConnectionAccepted += () => Console.WriteLine("accepted");
            client.ConnectionRejected += () => reject = true;
            client.ConnectionLost += () =>
            {
                Console.WriteLine("connection lost");
                conn();
            };
            
            void conn()
            {
                while (true)
                {
                    Console.WriteLine("connecting ...");
                    if (client.Connect())
                    {
                        Console.WriteLine("connection ok !");
                        break;
                    }
                    else Console.WriteLine("connect failed #");
                }
            }

            conn();

            while (true)
            {
                if (reject)
                {
                    Console.WriteLine("reject");
                    break;
                }
                Thread.Sleep(500);
            }
        }
    }
}
