using System;
using Infra.DataService.Protocol;

namespace TestData
{
    [Serializable]
    public class Data : LeafDataProtocol
    {
        public byte[] data;
        public Data(byte[] d) => data = d;
    }

    [Serializable]
    public class Rec : LeafDataProtocol
    {
        public long seq;
    }

    [Serializable]
    public class Done : LeafDataProtocol
    {
    }

}
