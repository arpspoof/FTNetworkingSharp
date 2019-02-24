using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Infra.Algorithm.StateMachine.UnitTest
{
    public class TestAction
    {
        List<string> results = new List<string>();

        public void CompareResults(List<string> expected)
        {
            if (results.Count != expected.Count)
                Assert.Fail($"different number of results, actual {results.Count}, expect {expected.Count}");
            for (int i = 0; i < results.Count; i++)
            {
                if (results[i] != expected[i]) 
                    Assert.Fail($"compare {i}: actual: {results[i]}, expect {expected[i]}");
            }
        }

        public void Monitor(State state)
        {
            state.StateEnter += () => results.Add($"enter: {state.Id}");
            state.StateLeave += () => results.Add($"leave: {state.Id}");
        }

        public void Clear() => results.Clear();
    }
}
