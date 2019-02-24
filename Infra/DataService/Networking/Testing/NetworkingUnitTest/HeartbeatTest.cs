using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Infra.DataService.Networking.UnitTest
{
    [TestClass]
    public class HeartBeatTest : IntegrationBase
    {
        [TestMethod]
        public void HeartBeat()
        {
            wire.Up();
            serverStatefulSession.heartBeatTimer.Tick();
            clientStatelessSession.heartBeatTimer.Tick();
            Assert.AreEqual(1, recoveryCount);
            Done();
        }
    }
}
