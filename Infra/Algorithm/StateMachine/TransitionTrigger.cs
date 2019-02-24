using System;

namespace Infra.Algorithm.StateMachine
{
    public class TransitionTrigger
    {
        internal event Action OnTrigger;
        internal bool Valid { get; private set; } = true;

        public event Action BeforeTransition;
        public event Action AfterTransition;

        public void Fire() { Valid = true; OnTrigger?.Invoke(); }
        public void StopTransition() => Valid = false;

        internal void FireBeforeTransition() => BeforeTransition?.Invoke();
        internal void FireAfterTransition() => AfterTransition?.Invoke();
    }
}
