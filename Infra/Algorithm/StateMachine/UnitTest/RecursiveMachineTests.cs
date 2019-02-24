using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Infra.Algorithm.StateMachine.UnitTest
{
    public class RecursiveMachineTestBase
    {
        protected TestAction action = new TestAction();
        protected TestSequentialTrigger trigger00, trigger01, trigger0, trigger010;
        protected StateMachine stateMachine00, stateMachine01, stateMachine010, stateMachine0;

        private void CreateSimpleStatesAndMonitor(TestAction action, StateMachine m, int n)
        {
            for (int i = 0; i < n; i++)
            {
                var state = new SimpleState();
                action.Monitor(state);
                m.AddState(state);
            }
            action.Monitor(m);
        }

        public RecursiveMachineTestBase()
        {
            trigger0 = new TestSequentialTrigger(2);
            trigger00 = new TestSequentialTrigger(3);
            trigger01 = new TestSequentialTrigger(4);
            trigger010 = new TestSequentialTrigger(2);

            stateMachine00 = new StateMachine();
            CreateSimpleStatesAndMonitor(action, stateMachine00, 3);
            stateMachine00.AddEntryTransition(0, null);
            stateMachine00.AddExitTransition(2, null);
            stateMachine00.AddTransition(0, 1, trigger00[0]);
            stateMachine00.AddTransition(2, 0, trigger00[2]);
            stateMachine00.AddTransition(1, 2, trigger00[1]);

            stateMachine010 = new StateMachine();
            CreateSimpleStatesAndMonitor(action, stateMachine010, 1);
            stateMachine010.AddEntryTransition(0, trigger010[0]);
            stateMachine010.AddExitTransition(0, trigger010[1]);

            stateMachine01 = new StateMachine();
            stateMachine01.AddState(stateMachine010);
            CreateSimpleStatesAndMonitor(action, stateMachine01, 1);
            stateMachine01.AddEntryTransition(0, trigger01[1]);
            stateMachine01.AddExitTransition(0, trigger01[2]);
            stateMachine01.AddTransition(0, 1, trigger01[3]);
            stateMachine01.AddTransition(1, 0, trigger01[0]);

            stateMachine0 = new StateMachine();
            stateMachine0.AddState(stateMachine00);
            stateMachine0.AddState(stateMachine01);
            CreateSimpleStatesAndMonitor(action, stateMachine0, 1);
            stateMachine0.AddEntryTransition(0, null); // No exit transition
            stateMachine0.AddTransition(0, 1, null);
            stateMachine0.AddTransition(1, 0, trigger0[1]);
            stateMachine0.AddTransition(1, 2, null);
            stateMachine0.AddTransition(2, 0, trigger0[0]);
        }
    }

    [TestClass]
    public class RecursiveMachineTransitionTests : RecursiveMachineTestBase
    {
        [TestMethod]
        public void RecursiveMachineTransitionTest()
        {
            stateMachine0.Run();
            Assert.AreEqual("0.0.-1", stateMachine0.GenerateActiveStateDescriptor());
            trigger00.Run(0, 1, 2);
            Assert.AreEqual("1.-2", stateMachine0.GenerateActiveStateDescriptor());
            trigger01.Run(1);
            Assert.AreEqual("1.0.-2", stateMachine0.GenerateActiveStateDescriptor());
            trigger010.Run(0);
            Assert.AreEqual("1.0.0.-1", stateMachine0.GenerateActiveStateDescriptor());
            trigger010.Run(1);
            Assert.AreEqual("1.0.-1", stateMachine0.GenerateActiveStateDescriptor());
            trigger01.Run(3);
            Assert.AreEqual("1.1.-1", stateMachine0.GenerateActiveStateDescriptor());
            trigger01.Run(0, 1, 2, 3); // some useless
            Assert.AreEqual("1.0.-2", stateMachine0.GenerateActiveStateDescriptor());
            trigger010.Run(0, 1);
            trigger01.Run(2);
            Assert.AreEqual("2.-1", stateMachine0.GenerateActiveStateDescriptor());
            trigger0.Run(0);
            Assert.AreEqual("0.0.-1", stateMachine0.GenerateActiveStateDescriptor());

            action.CompareResults(new List<string>()
            {
                "enter: 0", // outer state machine
                "enter: 0", // state 0 machine
                "enter: 0", "leave: 0",
                "enter: 1", "leave: 1",
                "enter: 2", "leave: 2",
                "leave: 0", // state 0 machine
                "enter: 1", // state 1 machine
                "enter: 0", // state 1 - state 0 machine
                "enter: 0", "leave: 0",
                "leave: 0", // state 1 - state 0 machine
                "enter: 1", "leave: 1",
                "enter: 0", // state 1 - state 0 machine
                "enter: 0", "leave: 0",
                "leave: 0", // state 1 - state 0 machine
                "leave: 1", // state 1 machine
                "enter: 2", "leave: 2",
                "enter: 0", // state 0 machine
                "enter: 0",
            });
        }
    }
}
