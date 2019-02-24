using System;
using Infra.DataService.Protocol;

namespace Infra.DataService.Networking
{
    [Serializable]
    public class FTConnectionProtocolHeader :  AbstractProtocol
    {
        public bool FlagRST = false; // reset
        public bool FlagREC = false; // recovery
        public bool FlagHTB = false; // heart beat

        private FTConnectionProtocolHeader() { }

        public static FTConnectionProtocolHeader RST => new FTConnectionProtocolHeader { FlagRST = true };
        public static FTConnectionProtocolHeader REC => new FTConnectionProtocolHeader { FlagREC = true };
        public static FTConnectionProtocolHeader HTB => new FTConnectionProtocolHeader { FlagHTB = true };
        public static FTConnectionProtocolHeader DAT => new FTConnectionProtocolHeader { };
    }
}
