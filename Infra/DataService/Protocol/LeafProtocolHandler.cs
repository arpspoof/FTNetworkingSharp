using System;

namespace Infra.DataService.Protocol
{
    [Serializable]
    public abstract class LeafDataProtocol : AbstractProtocol { }

    internal class LeafProtocolHandlerHelper<TProtocol> : AbstractProtocolHandler<TProtocol>
        where TProtocol : LeafDataProtocol
    {
        public event Action<TProtocol> NewData;
        public override event Action<TProtocol, AbstractProtocol> RequestSendToDownstream;

        public void Send(TProtocol protocol) => RequestSendToDownstream?.Invoke(protocol, null);
        public override TProtocol DownstreamProcess(object data) => null; // Not applicable
        public override void UpstreamProcess(TProtocol protocol) => NewData?.Invoke(protocol);
    }

    public class LeafProtocolHandler<TProtocol> where TProtocol : LeafDataProtocol
    {
        internal readonly LeafProtocolHandlerHelper<TProtocol> helper 
            = new LeafProtocolHandlerHelper<TProtocol>();

        public event Action<TProtocol> NewData;
        public LeafProtocolHandler() => helper.NewData += data => NewData?.Invoke(data);
        public void Send(TProtocol protocol) => helper.Send(protocol);
    }

    public static class LeafProtocolExtensions
    {
        public static void Register<TLeafProtocol>(
            this ProtocolTree tree, LeafProtocolHandler<TLeafProtocol> leaf)
            where TLeafProtocol : LeafDataProtocol => tree.Register(leaf.helper);

        public static void ConnectToLeaf<TParentProtocol, TLeafProtocol>(
            this ProtocolTree tree,
            AbstractProtocolHandler<TParentProtocol> parent,
            LeafProtocolHandler<TLeafProtocol> leaf)
            where TParentProtocol : AbstractProtocol
            where TLeafProtocol : LeafDataProtocol => tree.Connect(parent, leaf.helper);

        public static void EntryToLeaf<TLeafProtocol>(
            this ProtocolTree tree,
            LeafProtocolHandler<TLeafProtocol> leaf)
            where TLeafProtocol : LeafDataProtocol => tree.Entry(leaf.helper);
    }
}
