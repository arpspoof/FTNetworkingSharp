using System;
using System.Collections.Generic;

namespace Infra.DataService.Protocol
{
    internal class ProtocolTreeNode : IDataErrorGenerator,
        IDataReceiver<AbstractProtocol>, IDataEncapsulator<AbstractProtocol>,
        IDataSender<AbstractProtocol>
    {
        public object protocolHandler;
        public Dictionary<Type, ProtocolTreeNode> children =
            new Dictionary<Type, ProtocolTreeNode>();
        
        public event Action<AbstractProtocol> SenderDataReady;
        public event Action<DataIntegrityError> DataIntegrityError;
        public event Action<DataSymmetricityError> DataSymmetricityError;

        public void Encapsulate(AbstractProtocol data)
        {
            AbstractProtocol outerProtocol = ((dynamic)protocolHandler).DownstreamProcess((dynamic)data);
            outerProtocol.innerObject = data;
            SenderDataReady?.Invoke(outerProtocol);
        }

        public void PassThrough(AbstractProtocol data) => SenderDataReady?.Invoke(data);

        public void Receive(AbstractProtocol data)
        {
            ((dynamic)protocolHandler).UpstreamProcess((dynamic)data);
            data = data.innerObject;
            if (data != null)
            {
                if (!children.ContainsKey(data.GetType()))
                {
                    DataSymmetricityError?.Invoke(
                        new DataSymmetricityError("failed to demultiplex object, data type mismatch"));
                    return;
                }
                ProtocolTreeNode nextNode = children[data.GetType()];
                nextNode.Receive(data);
            }
        }

        public void ClearDownstream() => SenderDataReady = null;
    }

    [Serializable]
    internal class RootProtocol : AbstractProtocol { }

    public class ProtocolTree : IDataErrorGenerator,
        IDataReceiver<AbstractProtocol>, IDataSender<AbstractProtocol>
    {
        private ProtocolTreeNode root;
        private Dictionary<object, ProtocolTreeNode> map 
            = new Dictionary<object, ProtocolTreeNode>();

        public event Action<AbstractProtocol> SenderDataReady;
        public event Action<DataIntegrityError> DataIntegrityError;
        public event Action<DataSymmetricityError> DataSymmetricityError;

        public ProtocolTree()
        {
            root = new ProtocolTreeNode { protocolHandler = new DummyHandler<RootProtocol>() };
            root.SenderDataReady += obj => SenderDataReady?.Invoke(obj);
            AttachErrorHandler(root);
        }

        public void Receive(AbstractProtocol data) => root.Receive(data);

        public void Register<TProtocol>(AbstractProtocolHandler<TProtocol> protocolHandler)
            where TProtocol : AbstractProtocol
        {
            if (map.ContainsKey(protocolHandler)) return;
            var treeNode = new ProtocolTreeNode { protocolHandler = protocolHandler };
            AttachErrorHandler(treeNode);
            map.Add(protocolHandler, treeNode);
            protocolHandler.RequestSendToDownstream += (protocol, data) =>
            {
                protocol.innerObject = data;
                treeNode.PassThrough(protocol);
            };
        }

        public void Connect<TParentProtocol, TChildProtocol>(
            AbstractProtocolHandler<TParentProtocol> parentHandler,
            AbstractProtocolHandler<TChildProtocol> childHandler)
            where TParentProtocol : AbstractProtocol
            where TChildProtocol : AbstractProtocol
        {
            CheckKeyExist("Connect", parentHandler);
            CheckKeyExist("Connect", childHandler);
            AddChild(map[parentHandler], map[childHandler], typeof(TChildProtocol));
        }

        public void Connect<TParentProtocol>(
            AbstractProtocolHandler<TParentProtocol> parentHandler,
            ProtocolTree childTree)
            where TParentProtocol : AbstractProtocol
        {
            CheckKeyExist("Connect", parentHandler);
            AttachErrorHandler(childTree);
            foreach (var nodekvp in childTree.root.children)
                AddChild(map[parentHandler], nodekvp.Value, nodekvp.Key);
        }

        public void Entry<TProtocol>(AbstractProtocolHandler<TProtocol> protocolHandler)
            where TProtocol : AbstractProtocol
        {
            CheckKeyExist("Entry", protocolHandler);
            AddChild(root, map[protocolHandler], typeof(TProtocol));
        }

        public void Entry(ProtocolTree subTree)
        {
            AttachErrorHandler(subTree);
            foreach (var nodekvp in subTree.root.children)
                AddChild(root, nodekvp.Value, nodekvp.Key);
        }

        private void AttachErrorHandler(IDataErrorGenerator node)
        {
            node.DataIntegrityError += err => DataIntegrityError?.Invoke(err);
            node.DataSymmetricityError += err => DataSymmetricityError?.Invoke(err);
        }

        private void CheckKeyExist(string method, object key)
        {
            if (!map.ContainsKey(key))
                throw new DeveloperError("ProtocolTree", method, 
                    "ProtocolHandler not registered before connecting to other handlers",
                    "Please call ProtocolTree.Register on a handler first");
        }

        private void AddChild(ProtocolTreeNode parent, ProtocolTreeNode child, Type childType)
        {
            if (parent.children.ContainsKey(childType))
                throw new DeveloperError("ProtocolTree", "AddChild",
                    "Found different child node with same protocol type",
                    "Please make sure each child node is handling different protocols");
            parent.children.Add(childType, child);
            child.ClearDownstream();
            child.ConnectToDownstream(parent);
        }

        public void Detach()
        {
            foreach (var node in root.children.Values) node.ClearDownstream();
        }
    }
}
