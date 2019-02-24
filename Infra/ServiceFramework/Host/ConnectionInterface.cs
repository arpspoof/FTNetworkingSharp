using System;

namespace Infra.ServiceFramework.Host
{
    [Serializable]
    public class ConnectionInterface
    {
        public int Id { get; set; }
        public long Ticket { get; set; }
        public bool Enabled { get; set; }
    }
}
