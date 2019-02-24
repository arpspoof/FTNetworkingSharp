
namespace Infra.DataService.Networking
{
    public abstract partial class FTConnectionController
    {
        public static FTConnectionController CreateStatefulSession(
            ITransportationStateProvider transportationStateProvider, 
            IStateDataProvider stateDataProvider,
            int heartBeatTimeInterval, 
            int silenceTimeLimit)
        {
            FTConnectionController controller = new FTStatefulConnectionController(stateDataProvider);
            controller.Init(transportationStateProvider, heartBeatTimeInterval, silenceTimeLimit);
            return controller;
        }

        public static FTConnectionController CreateStatelessSession(
            ITransportationStateProvider transportationStateProvider,
            int heartBeatTimeInterval,
            int silenceTimeLimit)
        {
            FTConnectionController controller = new FTStatelessConnectionController();
            controller.Init(transportationStateProvider, heartBeatTimeInterval, silenceTimeLimit);
            return controller;
        }
    }
}
