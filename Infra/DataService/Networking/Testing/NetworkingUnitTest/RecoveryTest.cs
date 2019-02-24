using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Infra.DataService.Networking.UnitTest
{
    [TestClass]
    public class RecoveryTest : IntegrationBase
    {
        [TestMethod]
        public void RecoveryByTransportation()
        {
            wire.Up();
            Assert.AreEqual("1.-1", clientStatelessSession.StateDescr());
            Assert.AreEqual("1.-1", serverStatefulSession.StateDescr());

            MutualSend();

            wire.Down();
            Assert.AreEqual("0.-1", clientStatelessSession.StateDescr());
            Assert.AreEqual("0.-1", serverStatefulSession.StateDescr());
            MutualSend();

            wire.Up();
            MutualSend();

            wire.Down();
            Assert.AreEqual("0.-1", clientStatelessSession.StateDescr());
            Assert.AreEqual("0.-1", serverStatefulSession.StateDescr());
            MutualSend();

            wire.Up();
            MutualSend();

            Assert.AreEqual(3, serverDataCount);
            Assert.AreEqual(3, clientDataCount);
            Assert.AreEqual(3, recoveryCount);
            Done();
        }

        [TestMethod]
        public void RecoveryByTimer()
        {
            wire.Up();
            MutualSend();

            wire.SilentDown();
            MutualSend(); // nothing

            Assert.AreEqual(1, serverDataCount);
            Assert.AreEqual(1, clientDataCount);
            Assert.AreEqual(1, recoveryCount);

            clientStatelessSession.silenceTimer.Tick();
            Assert.AreEqual("0.-1", clientStatelessSession.StateDescr());
            Assert.AreEqual("1.-1", serverStatefulSession.StateDescr());

            serverStatefulSession.silenceTimer.Tick();
            Assert.AreEqual("0.-1", clientStatelessSession.StateDescr());
            Assert.AreEqual("0.-1", serverStatefulSession.StateDescr());

            wire.Up();
            Assert.AreEqual("1.-1", clientStatelessSession.StateDescr());
            Assert.AreEqual("1.-1", serverStatefulSession.StateDescr());

            MutualSend();

            Assert.AreEqual(2, serverDataCount);
            Assert.AreEqual(2, clientDataCount);
            Assert.AreEqual(2, recoveryCount);
            Done();
        }
    }
}
