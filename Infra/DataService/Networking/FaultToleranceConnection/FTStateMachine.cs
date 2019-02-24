using Infra.Algorithm.StateMachine;
using Infra.DataService.Protocol;

namespace Infra.DataService.Networking
{
    public abstract partial class FTConnectionController
    {
        protected StateMachine stateMachine = new StateMachine();
        protected SimpleState unconnectedState = new SimpleState();
        protected SimpleState connectedState = new SimpleState();
        protected SimpleState recoveryState = new SimpleState();
        
        protected TransitionTrigger transportationReadyTrigger = new TransitionTrigger();
        protected TransitionTrigger transportationLostTrigger = new TransitionTrigger();
        protected TransitionTrigger silenceTimeoutTrigger = new TransitionTrigger();
        protected TransitionTrigger recoveryTrigger = new TransitionTrigger();

        protected abstract void AddCustomTransitionsAndActions();

        private bool started = false;

        /*
         *  | state id |    state name    |
         *  |    0     |    unconnected   |
         *  |    1     |    connected     |
         *  |    2     |    recovery      |
         */
        private void BuildStateMachine(ITransportationStateProvider transportationStateProvider)
        {
            stateMachine.AddState(unconnectedState);
            stateMachine.AddState(connectedState);
            stateMachine.AddState(recoveryState);
            
            transportationStateProvider.ConnectionEstablished += () => transportationReadyTrigger.Fire();
            transportationStateProvider.ConnectionLost += () => transportationLostTrigger.Fire();

            unconnectedState.StateEnter += () =>
            {
                if (!started)
                {
                    started = true;
                    return;
                }
                Logger.Log($"unconnectedState.StateEnter, stop timers, state={StateDescr()}", "FT");
                silenceTimer.Stop();
                heartBeatTimer.Stop();
                Logger.Log($"invoking connection lost event, state={StateDescr()}", "FT");
                ConnectionLost?.Invoke();
            };
            unconnectedState.StateLeave += () =>
            {
                Logger.Log($"unconnectedState.StateLeave, start silence timer, state={StateDescr()}", "FT");
                silenceTimer.Start();
            };
            connectedState.StateEnter += () =>
            {
                Logger.Log($"connectedState.StateEnter, start heart beat, state={StateDescr()}", "FT");
                heartBeatTimer.Start();
            };

            stateMachine.AddEntryTransition(0, null);
            stateMachine.AddTransition(1, 0, silenceTimeoutTrigger);
            stateMachine.AddTransition(1, 0, transportationLostTrigger);

            AddCustomTransitionsAndActions();
            stateMachine.Run();
        }
        
        internal string StateDescr() => stateMachine.GenerateActiveStateDescriptor();
    }

    public interface IStateDataProvider
    {
        AbstractProtocol GetStateRecoveryData();
    }

    internal class FTStatefulConnectionController : FTConnectionController
    {
        private readonly IStateDataProvider stateDataProvider;

        public FTStatefulConnectionController(IStateDataProvider stateDataProvider)
            => this.stateDataProvider = stateDataProvider;

        protected override void AddCustomTransitionsAndActions()
        {
            stateMachine.AddTransition(0, 2, recoveryTrigger);
            stateMachine.AddTransition(1, 2, recoveryTrigger);
            stateMachine.AddTransition(2, 1, null);

            recoveryTrigger.AfterTransition += () =>
            {
                Logger.Log($"recoveryTrigger.AfterTransition, state={StateDescr()}", "FTStateful");
                SendInternalData(FTConnectionProtocolHeader.REC, stateDataProvider.GetStateRecoveryData());
            };
        }
    }

    internal class FTStatelessConnectionController : FTConnectionController
    {
        protected override void AddCustomTransitionsAndActions()
        {
            stateMachine.AddTransition(0, 2, transportationReadyTrigger);
            stateMachine.AddTransition(2, 0, silenceTimeoutTrigger);
            stateMachine.AddTransition(2, 0, transportationLostTrigger);
            stateMachine.AddTransition(2, 1, recoveryTrigger);

            transportationReadyTrigger.AfterTransition += () =>
            {
                Logger.Log($"transportationReadyTrigger.AfterTransition, state={StateDescr()}", "FTStateless");
                SendInternalData(FTConnectionProtocolHeader.REC);
            };
        }
    }
}
