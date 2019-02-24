using System;
using System.Collections.Generic;
using Infra.DataService.Protocol;
using static Infra.Logger;

namespace Infra.ServiceFramework.Host
{
    public partial class ServiceHost : IHostControl
    {
        [Serializable]
        private class ServiceBackupData : AbstractProtocol
        {
            public List<ConnectionInterface> connectionInterfaces;
        }

        private readonly ServiceBackupData serviceBackupData = new ServiceBackupData();
        private readonly IServiceLogic logic;
        private readonly object interfaceLock = new object();

        public bool CloseInterface(int index)
        {
            lock (interfaceLock)
            {
                if (serviceBackupData.connectionInterfaces[index].Enabled == false) return false;
                serviceBackupData.connectionInterfaces[index].Enabled = false;
                return true;
            }
        }

        public bool OpenInterface(int index)
        {
            lock (interfaceLock)
            {
                if (serviceBackupData.connectionInterfaces[index].Enabled == true) return false;
                serviceBackupData.connectionInterfaces[index].Enabled = true;
                return true;
            }
        }

        public ServiceHost(IServiceLogic logic)
        {
            this.logic = logic;
            logic.Host = this;
            InitConnectionInterfaces();
            logic.Init();
        }

        private void InitConnectionInterfaces()
        {
            serviceBackupData.connectionInterfaces = new List<ConnectionInterface>();
            for (int i = 0; i < logic.MaxConnection; i++)
            {
                var connection = new ConnectionInterface();
                connection.Id = i;
                connection.Ticket = /*Rnd64()*/i;
                connection.Enabled = true;
                serviceBackupData.connectionInterfaces.Add(connection);
            }
        }

        private long Rnd64()
        {
            Random rnd = new Random();
            return (long)rnd.Next() * rnd.Next() * rnd.Next();
        }

        private void Log(string str, LogType type = LogType.INFO)
        {
            if (type == LogType.INFO) Logger.Log(str, "ServiceHost", LogType.INFO, ConsoleColor.Green);
            else Logger.Log(str, "ServiceHost", type);
        }
    }
}
