using System;
using System.Threading.Tasks;
using Infra.ServiceFramework.Client;

namespace NumGame.Logic
{
    public class ClientLogic : AbstractClientLogic
    {
        public int id;
        public int mynum;

        public ClientLogic(int id)
        {
            this.id = id;

            void myturn()
            {
                new Task(() =>
                {
                    Console.WriteLine($"it's my turn, please input {mynum}");
                    try
                    {
                        int x = Convert.ToInt32(Console.ReadLine().Trim());
                        Send(new Num { x = x });
                    }
                    catch
                    {
                        Console.WriteLine("please input again");
                        myturn();
                    }
                }).Start();
            }

            RegisterDataType<Num>(null);
            RegisterDataType<Count>(c => Console.WriteLine($"current players: {c.cnt}"));
            RegisterDataType<Err>(e =>
            {
                Console.WriteLine(e.msg);
                myturn();
            });
            RegisterDataType<Turn>(t =>
            {
                if (t.i == id)
                {
                    mynum = t.num;
                    myturn();
                }
                else
                {
                    Console.WriteLine($"it's {t.i} turn");
                }
            });
            RegisterDataType<End>(n => Console.WriteLine("game ended"));
        }
    }
}
