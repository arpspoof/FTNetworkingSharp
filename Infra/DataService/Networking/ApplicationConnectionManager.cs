using System;
using Infra.DataService.Protocol;
using static Infra.Logger;

namespace Infra.DataService.Networking
{
    public class ApplicationConnectionManager : IDataErrorGenerator, IDisposable
    {
        private readonly ITransportationLayer transportationLayer;
        private readonly FTConnectionController ft;
        private readonly ProtocolTree tree = new ProtocolTree();
        private readonly DataSerializer dataSerializer = new DataSerializer();

        private void CloseConnectionAndCleanUp()
        {
            transportationLayer.CloseConnection();
            dataSerializer.ClearBuffer();
        }

        public event Action ConnectionLost;
        public event Action<DataIntegrityError> DataIntegrityError;
        public event Action<DataSymmetricityError> DataSymmetricityError;

        public void OnDataIntegrityError(DataIntegrityError err)
        {
            Log($"DataIntegrityError: {err.Message}", "App", LogType.ERROR);
            CloseConnectionAndCleanUp();
            DataIntegrityError?.Invoke(err);
        }
        public void OnDataSymmetricityError(DataSymmetricityError err)
        {
            Log($"DataSymmetricityError: {err.Message}", "App", LogType.ERROR);
            CloseConnectionAndCleanUp();
            DataSymmetricityError?.Invoke(err);
        }

        public void Dispose()
        {
            CloseConnectionAndCleanUp();
            if (ft != null) ft.Dispose();
        }

        private ApplicationConnectionManager(ITransportationLayer transportationLayer)
        {
            this.transportationLayer = transportationLayer;

            tree.DataIntegrityError += OnDataIntegrityError;
            tree.DataSymmetricityError += OnDataSymmetricityError;

            dataSerializer.DataIntegrityError += OnDataIntegrityError;
            dataSerializer.DataSymmetricityError += OnDataSymmetricityError;

            transportationLayer.ConnectToUpstream(dataSerializer);
            dataSerializer.ConnectToDownstream(transportationLayer);

            dataSerializer.ConnectToUpstream(tree);
            tree.ConnectToDownstream(dataSerializer);
        }

        private ApplicationConnectionManager(ITransportationLayer transportationLayer, 
            ProtocolTree protocolTree, FTConnectionController ft) : this(transportationLayer)
        {
            this.ft = ft;

            tree.Register(ft);
            tree.Entry(ft);
            tree.Connect(ft, protocolTree);

            ft.ConnectionLost += () =>
            {
                CloseConnectionAndCleanUp();
                ConnectionLost?.Invoke();
            };
        }

        public ApplicationConnectionManager(ITransportationLayer transportationLayer, ProtocolTree protocolTree)
            : this(transportationLayer)
        {
            tree.Entry(protocolTree);
            transportationLayer.ConnectionLost += CloseConnectionAndCleanUp;
        }

        public ApplicationConnectionManager(ITransportationLayer transportationLayer, ProtocolTree protocolTree,
            int heartBeatTimeInterval, int silenceTimeLimit)
            : this(transportationLayer, protocolTree, FTConnectionController.CreateStatelessSession(
                transportationLayer, heartBeatTimeInterval, silenceTimeLimit)) { }

        public ApplicationConnectionManager(ITransportationLayer transportationLayer, ProtocolTree protocolTree,
            IStateDataProvider stateDataProvider, int heartBeatTimeInterval, int silenceTimeLimit)
            : this(transportationLayer, protocolTree, FTConnectionController.CreateStatefulSession(
                transportationLayer, stateDataProvider, heartBeatTimeInterval, silenceTimeLimit)) { }
    }
}
