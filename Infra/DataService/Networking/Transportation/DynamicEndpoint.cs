using System;
using System.IO;

namespace Infra.DataService.Networking
{
    public abstract class DynamicEndpoint : ITransportationLayer
    {
        protected readonly dynamic client;

        public event Action ConnectionEstablished;
        public event Action ConnectionLost;
        public event Action<Stream> ProviderDataReady;

        public DynamicEndpoint(dynamic client)
        {
            this.client = client;
            ITransportationLayer transport = client;
            transport.ConnectionEstablished += () => ConnectionEstablished?.Invoke();
            transport.ConnectionLost += () => ConnectionLost?.Invoke();
            transport.ProviderDataReady += data => ProviderDataReady?.Invoke(data);
        }

        public void CloseConnection() => client.CloseConnection();
        public void Encapsulate(Stream data) => client.Encapsulate(data);
    }
}
