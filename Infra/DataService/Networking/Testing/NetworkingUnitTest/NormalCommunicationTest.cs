using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Infra.DataService.Networking.UnitTest
{
    [TestClass]
    public class NormalCommunicationTest : IntegrationBase
    {
        [TestMethod]
        public void NormalCommunication()
        {
            Assert.AreEqual(5000, serverStatefulSession.HeartBeatTimeInterval);
            Assert.AreEqual(5000, clientStatelessSession.HeartBeatTimeInterval);
            Assert.AreEqual(8000, serverStatefulSession.SilenceTimeLimit);
            Assert.AreEqual(8000, clientStatelessSession.SilenceTimeLimit);
            wire.Up();
            for (int i = 0; i < 100; i++)
            {
                ServerSend();
                ClientSend();
                ServerSend();
                ServerSend();
                ClientSend();
            }
            Assert.AreEqual(200, serverDataCount);
            Assert.AreEqual(300, clientDataCount);
            Assert.AreEqual(1, recoveryCount);
            Done();
        }
    }
}
