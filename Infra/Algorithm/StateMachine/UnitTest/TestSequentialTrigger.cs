using System.Collections.Generic;

namespace Infra.Algorithm.StateMachine.UnitTest
{
    public class TestSequentialTrigger
    {
        List<TransitionTrigger> triggers;

        public TestSequentialTrigger(int n)
        {
            triggers = new List<TransitionTrigger>();
            for (int i = 0; i < n; i++) triggers.Add(new TransitionTrigger());
        }

        public TransitionTrigger this[int i] => triggers[i];

        public void Run(params int[] sq)
        {
            for (int i = 0; i < sq.Length; i++) triggers[sq[i]].Fire();
        }
    }
}
