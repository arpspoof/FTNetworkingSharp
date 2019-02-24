using System;
using Infra.DataService.Protocol;

namespace Infra.ServiceFramework.Client
{
    public abstract class AbstractClientLogic : IClientLogic
    {
        private readonly SimpleTreeBuilder treeBuilder = new SimpleTreeBuilder();

        public ProtocolTree ProtocolTree { get => treeBuilder.Tree; }

        protected void RegisterDataType<TData>(Action<TData> dataHandler) where TData : LeafDataProtocol
        {
            treeBuilder.RegisterDataType(dataHandler);
        }

        protected void Send<TData>(TData data) where TData : LeafDataProtocol
        {
            treeBuilder.Send(data);
        }
    }
}
