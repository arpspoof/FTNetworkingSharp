using System;

namespace Infra.Algorithm.StateMachine
{
    internal class StateNode
    {
        public event Action StateNodeEnter;
        public event Action StateNodeLeave;

        private event Action StateNodeEntered;
        private event Action StateNodeLeaved;

        public bool Active { get; private set; }

        public void Enter()
        {
            StateNodeEnter?.Invoke();
            Active = true;
            StateNodeEntered?.Invoke();
        }

        public void RecoveryEnter()
        {
            Active = true;
            StateNodeEntered?.Invoke();
        }

        public void Leave()
        {
            StateNodeLeave?.Invoke();
            Active = false;
            StateNodeLeaved?.Invoke();
        }

        public void Reset() => Active = false;

        public void AddTransitionTo(StateNode other, TransitionTrigger trigger)
        {
            if (trigger == null)
            {
                StateNodeEntered = null;
                StateNodeEntered += () => Goto(other);
            }
            else
            {
                trigger.OnTrigger += () =>
                {
                    if (Active)
                    {
                        trigger.FireBeforeTransition();
                        if (trigger.Valid)
                        {
                            trigger.StopTransition();
                            Goto(other);
                            trigger.FireAfterTransition();
                        }
                    }
                };
            }
        }

        private void Goto(StateNode other)
        {
            this.Leave(); other.Enter();
        }
    }
}
