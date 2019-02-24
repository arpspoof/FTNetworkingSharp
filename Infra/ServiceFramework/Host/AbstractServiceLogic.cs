using System;
using System.Collections.Generic;
using Infra.DataService.Protocol;

namespace Infra.ServiceFramework.Host
{
    public abstract class AbstractServiceLogic : IServiceLogic
    {
        private class Handlers
        {
            public int id;
            public SimpleTreeBuilder treeBuilder = new SimpleTreeBuilder();
        }

        private readonly List<Handlers> handlers = new List<Handlers>();
        public AbstractServiceLogic()
        {
            for (int i = 0; i < MaxConnection; i++)
            {
                handlers.Add(new Handlers { id = i });
            }
        }

        public IHostControl Host { get; set; }
        public ProtocolTree GetProtocolTree(int index) => handlers[index].treeBuilder.Tree;

        public abstract int MaxConnection { get; }
        public abstract void OnClientEnter(int index);
        public abstract void OnClientLeave(int index);
        public abstract bool CanResume(int index);
        public abstract void OnClientDisconnect(int index);
        public abstract void OnClientResume(int index);
        public abstract void Init();

        protected bool OpenInterface(int index) => Host.OpenInterface(index);
        protected bool CloseInterface(int index) => Host.CloseInterface(index);

        protected void RegisterDataType<TData>(Action<int, TData> dataHandler) where TData : LeafDataProtocol
        {
            for (int i = 0; i < MaxConnection; i++)
            {
                Handlers h = handlers[i];
                h.treeBuilder.RegisterDataType<TData>(data => dataHandler?.Invoke(h.id, data));
            }
        }

        protected void Send<TData>(int i, TData data) where TData : LeafDataProtocol
        {
            handlers[i].treeBuilder.Send(data);
        }
    }
}
