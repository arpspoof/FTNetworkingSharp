using System;

namespace Infra.Algorithm.StateMachine
{
    public abstract class State
    {
        public static readonly int InvalidStateId = -3;
        public static readonly int EntryStateId = -2;
        public static readonly int ExitStateId = -1;

        internal StateNode EntryNode = new StateNode();
        internal StateNode ExitNode = new StateNode();

        public event Action StateEnter;
        public event Action StateLeave;

        public int Id { get; set; }
        public int ActiveStateId { get; protected set; } = InvalidStateId;

        protected State()
        {
            EntryNode.StateNodeEnter += () => { ActiveStateId = EntryStateId; StateEnter?.Invoke(); };
            ExitNode.StateNodeEnter += () => { ActiveStateId = ExitStateId; };
            ExitNode.StateNodeLeave += () => { StateLeave?.Invoke(); ActiveStateId = InvalidStateId; };
        }

        protected void ResetSelf()
        {
            ActiveStateId = InvalidStateId;
            EntryNode.Reset(); ExitNode.Reset();
        }

        public abstract void Reset();
        public abstract string GenerateActiveStateDescriptor();
        internal abstract void RecoverFromStateDescriptor(string stateDescriptor);

        internal void AddTransitionTo(State other, TransitionTrigger trigger) 
            => this.ExitNode.AddTransitionTo(other.EntryNode, trigger);
    }

    public class SimpleState : State
    {
        public SimpleState() => EntryNode.AddTransitionTo(ExitNode, null);
        public override void Reset() => ResetSelf();
        public override string GenerateActiveStateDescriptor() => ActiveStateId.ToString();
        internal override void RecoverFromStateDescriptor(string stateDescriptor)
        {
            try
            {
                ActiveStateId = Convert.ToInt32(stateDescriptor);
                if (ActiveStateId == EntryStateId) EntryNode.RecoveryEnter();
                else if (ActiveStateId == ExitStateId) ExitNode.RecoveryEnter();
                else throw new ArgumentException();
            }
            catch
            {
                throw new ArgumentException("Infra.Algorithm.StateMachine.StateMachine: stateDescriptor is invalid");
            }
        }
    }
}
