using System;
using System.IO;
using Infra.DataService.Protocol;

namespace Infra.DataService.Networking
{
    public interface ITransportationStateProvider
    {
        event Action ConnectionEstablished;
        event Action ConnectionLost;
    }

    public interface ITransportationLayer : ITransportationStateProvider,
        IDataEncapsulator<Stream>, IDataProvider<Stream>
    {
        void CloseConnection();
    }
}
