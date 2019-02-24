using System.Net;
using System.Threading;
using Infra.ServiceFramework.Host;
using NumGame.Logic;

namespace NumGame.Host
{
    class Host
    {
        static void Main(string[] args)
        {
            Infra.Logger.Filter.Add("TCP");
            Infra.Logger.Filter.Add("DS");
            Infra.Logger.Filter.Add("FT");
            HostLogic logic = new HostLogic();
            ServiceHost serviceHost = new ServiceHost(logic);
            serviceHost.InitClientListenerByTCP(IPAddress.Parse("192.168.1.73"), 54321);
            while (true) Thread.Sleep(500);
        }
    }
}
