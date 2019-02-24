using System;
using System.Collections.Generic;

namespace Infra.DataService.Protocol
{
    public class SimpleTreeBuilder
    {
        private readonly Dictionary<Type, object> handlers = new Dictionary<Type, object>();

        public ProtocolTree Tree { get; } = new ProtocolTree();

        public void RegisterDataType<TData>(Action<TData> dataHandler) where TData : LeafDataProtocol
        {
            if (handlers.ContainsKey(typeof(TData)))
                throw new DeveloperError("SimpleTreeBuilder", "RegisterDataType",
                    "duplicated data types", "one type can only be registered once");
            LeafProtocolHandler<TData> handler = new LeafProtocolHandler<TData>();
            handlers.Add(typeof(TData), handler);
            Tree.Register(handler);
            Tree.EntryToLeaf(handler);
            handler.NewData += data => dataHandler?.Invoke(data);
        }

        public void Send<TData>(TData data) where TData : LeafDataProtocol
        {
            if (!handlers.ContainsKey(typeof(TData)))
                throw new DeveloperError("SimpleTreeBuilder", "Send",
                    "data type not registered", "each type should be registered first by [RegisterDataType]");
            var handler = (LeafProtocolHandler<TData>)handlers[typeof(TData)];
            handler.Send(data);
        }
    }
}
