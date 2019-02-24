using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Infra.Algorithm.StateMachine.UnitTest
{
    [TestClass]
    public class SimpleStateTests
    {
        [TestMethod]
        public void SimpleStatesOnly()
        {
            TestSequentialTrigger trigger = new TestSequentialTrigger(8);
            TestAction action = new TestAction();
            StateMachine stateMachine = new StateMachine();
            for (int i = 0; i < 4; i++)
            {
                var state = new SimpleState();
                action.Monitor(state);
                stateMachine.AddState(state);
            }
            action.Monitor(stateMachine);
            stateMachine.AddEntryTransition(0, trigger[7]);
            stateMachine.AddExitTransition(1, trigger[6]);
            stateMachine.AddTransition(0, 1, trigger[0]);
            stateMachine.AddTransition(0, 3, trigger[1]);
            stateMachine.AddTransition(1, 2, trigger[4]);
            stateMachine.AddTransition(2, 0, trigger[3]);
            stateMachine.AddTransition(3, 1, trigger[5]);
            stateMachine.AddTransition(3, 2, trigger[2]);
            Assert.AreEqual(State.InvalidStateId, stateMachine.ActiveStateId);
            Assert.AreEqual("-3", stateMachine.GenerateActiveStateDescriptor());
            stateMachine.Run();
            Assert.AreEqual(State.EntryStateId, stateMachine.ActiveStateId);
            Assert.AreEqual("-2", stateMachine.GenerateActiveStateDescriptor());
            trigger.Run(7, 1, 2, 3, 0, 4, 3, 1, 5); // loop around
            Assert.AreEqual("1.-1", stateMachine.GenerateActiveStateDescriptor());
            trigger.Run(0, 1, 2, 3, 5); // invalid triggers
            Assert.AreEqual("1.-1", stateMachine.GenerateActiveStateDescriptor());
            trigger.Run(6); // to the end
            Assert.AreEqual("-1", stateMachine.GenerateActiveStateDescriptor());
            action.CompareResults(new List<string>()
            {
                "enter: 0", // outer state machine
                "enter: 0", "leave: 0", // state 0
                "enter: 3", "leave: 3",
                "enter: 2", "leave: 2",
                "enter: 0", "leave: 0",
                "enter: 1", "leave: 1",
                "enter: 2", "leave: 2",
                "enter: 0", "leave: 0",
                "enter: 3", "leave: 3",
                "enter: 1", "leave: 1",
            });
            Assert.AreEqual(State.ExitStateId, stateMachine.ActiveStateId);
        }

        [TestMethod]
        public void EpsilonTransitions()
        {
            TestSequentialTrigger trigger = new TestSequentialTrigger(1);
            TestAction action = new TestAction();
            StateMachine stateMachine = new StateMachine();
            for (int i = 0; i < 4; i++)
            {
                var state = new SimpleState();
                action.Monitor(state);
                stateMachine.AddState(state);
            }
            action.Monitor(stateMachine);
            stateMachine.AddEntryTransition(0, null);
            stateMachine.AddExitTransition(3, null);
            stateMachine.AddTransition(0, 1, null);
            stateMachine.AddTransition(1, 2, trigger[0]);
            stateMachine.AddTransition(2, 3, null);
            stateMachine.Run();
            Assert.AreEqual(1, stateMachine.ActiveStateId);
            trigger.Run(0);
            action.CompareResults(new List<string>()
            {
                "enter: 0",
                "enter: 0", "leave: 0",
                "enter: 1", "leave: 1",
                "enter: 2", "leave: 2",
                "enter: 3", "leave: 3",
            });
            Assert.AreEqual(State.ExitStateId, stateMachine.ActiveStateId);
        }

        [TestMethod]
        public void TransitionEvents()
        {
            StateMachine stateMachine = new StateMachine();
            SimpleState s0 = new SimpleState(), s1 = new SimpleState();
            stateMachine.AddState(s0);
            stateMachine.AddState(s1);
            stateMachine.AddEntryTransition(0, null);
            stateMachine.AddExitTransition(1, null);
            TransitionTrigger trigger = new TransitionTrigger();
            stateMachine.AddTransition(0, 1, trigger);
            List<string> result = new List<string>();
            s0.StateLeave += () => result.Add("leave: 0");
            s1.StateEnter += () => result.Add("enter: 1");
            trigger.BeforeTransition += () =>
            {
                result.Add("before transition");
                Assert.AreEqual("0.-1", stateMachine.GenerateActiveStateDescriptor());
            };
            trigger.AfterTransition += () =>
            {
                result.Add("after transition");
                Assert.AreEqual("-1", stateMachine.GenerateActiveStateDescriptor());
            };
            stateMachine.Run();
            trigger.Fire();
            Assert.AreEqual(4, result.Count);
            Assert.AreEqual("before transition", result[0]);
            Assert.AreEqual("leave: 0", result[1]);
            Assert.AreEqual("enter: 1", result[2]);
            Assert.AreEqual("after transition", result[3]);
            
            trigger.BeforeTransition += () => trigger.StopTransition();
            result.Clear();
            stateMachine.Reset();
            stateMachine.Run();
            trigger.Fire();
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("before transition", result[0]);
        }

        [TestMethod]
        public void MultiEdges()
        {
            StateMachine stateMachine = new StateMachine();
            SimpleState s0 = new SimpleState(), s1 = new SimpleState();
            stateMachine.AddState(s0);
            stateMachine.AddState(s1);
            stateMachine.AddEntryTransition(0, null);
            stateMachine.AddExitTransition(1, null);
            TransitionTrigger trigger0 = new TransitionTrigger();
            TransitionTrigger trigger1 = new TransitionTrigger();
            stateMachine.AddTransition(0, 1, trigger0);
            stateMachine.AddTransition(0, 1, trigger1);

            stateMachine.Run();
            trigger0.Fire();
            Assert.AreEqual("-1", stateMachine.GenerateActiveStateDescriptor());
            stateMachine.Reset();
            stateMachine.Run();
            trigger1.Fire();
            Assert.AreEqual("-1", stateMachine.GenerateActiveStateDescriptor());
        }
    }
}
