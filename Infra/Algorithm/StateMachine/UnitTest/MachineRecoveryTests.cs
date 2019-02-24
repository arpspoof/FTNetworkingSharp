using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Infra.Algorithm.StateMachine.UnitTest
{
    [TestClass]
    public class MachineRecoveryTests : RecursiveMachineTestBase
    {
        [TestMethod]
        public void MachineRecoveryTest()
        {
            action.Clear();
            stateMachine0.RecoverState("-2");
            Assert.AreEqual("0.0.-1", stateMachine0.GenerateActiveStateDescriptor());
            action.CompareResults(new List<string>()
            {
                "enter: 0", // state 0 machine
                "enter: 0",
            });

            action.Clear();
            stateMachine0.RecoverState("-1");
            Assert.AreEqual("-1", stateMachine0.GenerateActiveStateDescriptor());
            action.CompareResults(new List<string>());

            action.Clear();
            stateMachine0.RecoverState("0.-2");
            Assert.AreEqual("0.0.-1", stateMachine0.GenerateActiveStateDescriptor());
            action.CompareResults(new List<string>()
            {
                "enter: 0",
            });

            action.Clear();
            stateMachine0.RecoverState("0.2.-1");
            Assert.AreEqual("1.-2", stateMachine0.GenerateActiveStateDescriptor());
            action.CompareResults(new List<string>()
            {
                "leave: 2",
                "leave: 0", // state 0 machine
                "enter: 1", // state 1 machine
            });

            action.Clear(); 
            stateMachine0.RecoverState("0.2.-2");
            Assert.AreEqual("1.-2", stateMachine0.GenerateActiveStateDescriptor());
            action.CompareResults(new List<string>()
            {
                "leave: 2",
                "leave: 0", // state 0 machine
                "enter: 1", // state 1 machine
            });

            action.Clear();
            stateMachine0.RecoverState("1.0.0.-1");
            trigger01.Run(1); // should have no effect
            Assert.AreEqual("1.0.0.-1", stateMachine0.GenerateActiveStateDescriptor());
            action.CompareResults(new List<string>());

            action.Clear();
            stateMachine0.RecoverState("1.-1");
            Assert.AreEqual("2.-1", stateMachine0.GenerateActiveStateDescriptor());
            action.CompareResults(new List<string>()
            {
                "leave: 1", // state 1 machine
                "enter: 2",
            });

            action.Clear();
            stateMachine0.RecoverState("1.1.-2");
            trigger0.Run(0); // should have no effect
            Assert.AreEqual("1.1.-1", stateMachine0.GenerateActiveStateDescriptor());
            action.CompareResults(new List<string>());
        }

        [TestMethod]
        public void InvalidDescriptorTest()
        {
            Assert.ThrowsException<ArgumentException>(() => stateMachine0.RecoverState("-3"));
            Assert.ThrowsException<ArgumentException>(() => stateMachine0.RecoverState("4"));
            Assert.ThrowsException<ArgumentException>(() => stateMachine0.RecoverState("0.0.0.-1"));
            Assert.ThrowsException<ArgumentException>(() => stateMachine0.RecoverState("0.-3"));
            Assert.ThrowsException<ArgumentException>(() => stateMachine0.RecoverState("0.5.-1"));
            Assert.ThrowsException<ArgumentException>(() => stateMachine0.RecoverState("test"));
        }
    }
}
