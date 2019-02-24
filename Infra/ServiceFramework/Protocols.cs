using System;
using Infra.DataService.Protocol;

namespace Infra.ServiceFramework
{
    [Serializable]
    public class DummyProtocol : AbstractProtocol { }

    [Serializable]
    public class AuthenticationProtocol : LeafDataProtocol
    {
        public enum StatusCode { Request, Ack, Accept, Reject }
        public StatusCode statusCode;
        public int interfaceId;
        public long ticket;
        public long resumeToken;
        public string reason;
    }

    [Serializable]
    public class StateRecoveryProtocol : LeafDataProtocol
    {
        public enum StatusCode { Request, Ack }
        public StatusCode statusCode;
        public AbstractProtocol data;
    }
}
