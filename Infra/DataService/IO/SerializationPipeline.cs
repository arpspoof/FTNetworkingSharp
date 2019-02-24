using System.Collections.Generic;
using Infra.DataService.Protocol;

namespace Infra.DataService.IO
{
    public class SerializationPipeline : IAbstractIO
    {
        private readonly DataSerializer serializer = new DataSerializer();
        private readonly AbstractIODevice device;

        public SerializationPipeline(AbstractIODevice device)
        {
            this.device = device;
            serializer.ConnectToDownstream(device);
            device.ConnectToUpstream(serializer);
        }

        public void Write(AbstractProtocol protocol) => serializer.Encapsulate(protocol);

        public List<AbstractProtocol> Read()
        {
            List<AbstractProtocol> protocols = new List<AbstractProtocol>();
            void OnData(AbstractProtocol protocol) => protocols.Add(protocol);
            serializer.ProviderDataReady += OnData;
            device.Read();
            serializer.ProviderDataReady -= OnData;
            return protocols;
        }
    }
}
