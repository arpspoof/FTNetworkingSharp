using System;

namespace Infra.DataService.Protocol
{
    [Serializable]
    public abstract class AbstractProtocol
    {
        internal AbstractProtocol innerObject;
    }

    public abstract class AbstractProtocolHandler<TProtocol>
        where TProtocol : AbstractProtocol
    {
        public abstract event Action<TProtocol, AbstractProtocol> RequestSendToDownstream;

        public abstract void UpstreamProcess(TProtocol protocol);
        public abstract TProtocol DownstreamProcess(object data);
    }

    public class DummyHandler<TProtocol> : AbstractProtocolHandler<TProtocol>
        where TProtocol : AbstractProtocol, new()
    {
        public override event Action<TProtocol, AbstractProtocol> RequestSendToDownstream;
        public override TProtocol DownstreamProcess(object data) => new TProtocol();
        public override void UpstreamProcess(TProtocol protocol) { }
    }
}
