using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Infra.DataService.Networking.UnitTest
{
    [TestClass]
    public class ResetTest : IntegrationBase
    {
        [TestMethod]
        public void ClientReset()
        {
            bool reset = false;
            wire.Up();
            serverStatefulSession.ConnectionRefused += () => reset = true;
            clientStatelessSession.ResetConnection();
            Assert.IsTrue(reset);
            Done();
        }

        [TestMethod]
        public void ServerReset()
        {
            bool reset = false;
            wire.Up();
            clientStatelessSession.ConnectionRefused += () => reset = true;
            serverStatefulSession.ResetConnection();
            Assert.IsTrue(reset);
            Done();
        }
    }
}
