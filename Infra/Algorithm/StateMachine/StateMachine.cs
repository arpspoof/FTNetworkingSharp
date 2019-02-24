using System;
using System.Collections.Generic;

namespace Infra.Algorithm.StateMachine
{
    public class StateMachine : State
    {
        private readonly List<State> states = new List<State>();

        public int AddState(State state)
        {
            int id = states.Count;
            state.Id = id;
            states.Add(state);
            state.StateEnter += () => { ActiveStateId = id; };
            return id;
        }

        public void AddTransition(int idFrom, int idTo, TransitionTrigger trigger)
        {
            CheckArgumentId(idFrom);
            CheckArgumentId(idTo);
            states[idFrom].AddTransitionTo(states[idTo], trigger);
        }

        public void AddEntryTransition(int idTo, TransitionTrigger trigger)
        {
            CheckArgumentId(idTo);
            EntryNode.AddTransitionTo(states[idTo].EntryNode, trigger);
        }

        public void AddExitTransition(int idFrom, TransitionTrigger trigger)
        {
            CheckArgumentId(idFrom);
            states[idFrom].ExitNode.AddTransitionTo(ExitNode, trigger);
        }

        private void CheckArgumentId(int id)
        {
            if (id < 0 || id >= states.Count)
                throw new ArgumentException("Infra.Algorithm.StateMachine.StateMachine: state id out of bound");
        }

        public override string GenerateActiveStateDescriptor()
        {
            string str = $"{ActiveStateId.ToString()}";
            if (ActiveStateId >= 0) str += $".{states[ActiveStateId].GenerateActiveStateDescriptor()}";
            return str;
        }

        internal override void RecoverFromStateDescriptor(string stateDescriptor)
        {
            try
            {
                int indexOfDot = stateDescriptor.IndexOf('.');
                string substr = indexOfDot >= 0 ? stateDescriptor.Substring(0, indexOfDot) : stateDescriptor;
                ActiveStateId = Convert.ToInt32(substr);
                if (ActiveStateId == EntryStateId) EntryNode.RecoveryEnter();
                else if (ActiveStateId == ExitStateId) ExitNode.RecoveryEnter();
                else states[ActiveStateId].RecoverFromStateDescriptor(stateDescriptor.Substring(indexOfDot + 1));
            }
            catch
            {
                throw new ArgumentException("Infra.Algorithm.StateMachine.StateMachine: stateDescriptor is invalid");
            }
        }

        public void Run()
        {
            ActiveStateId = EntryStateId;
            EntryNode.Enter();
        }

        public override void Reset()
        {
            ResetSelf();
            foreach (State s in states) s.Reset();
        }

        public void RecoverState(string stateDescriptor)
        {
            Reset();
            RecoverFromStateDescriptor(stateDescriptor);
        }
    }
}
