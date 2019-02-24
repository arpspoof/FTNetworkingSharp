using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Infra.DataService.Protocol.UnitTest
{
    [Serializable]
    public class MockData1 : LeafDataProtocol
    {
        int[] x = new int[3] { 1, 2, 3 };
        string y = "asd";
        object z = null;
        public void Verify(Logger lg = null)
        {
            Assert.AreEqual(1, x[0]);
            Assert.AreEqual(2, x[1]);
            Assert.AreEqual(3, x[2]);
            Assert.AreEqual("asd", y);
            Assert.IsNull(z);
            if (lg != null) lg.Log("data1");
        }
    }

    [Serializable]
    public class MockData2 : LeafDataProtocol
    {
        List<double> lst = new List<double> { 1.2, 3.5 };
        Dictionary<string, bool> d = new Dictionary<string, bool> { { "as", false }, { "df", true } };
        public void Verify(Logger lg = null)
        {
            Assert.AreEqual(2, lst.Count);
            Assert.AreEqual(1.2, lst[0]);
            Assert.AreEqual(3.5, lst[1]);
            Assert.AreEqual(2, d.Count);
            Assert.AreEqual(false, d["as"]);
            Assert.AreEqual(true, d["df"]);
            if (lg != null) lg.Log("data2");
        }
    }

    [Serializable]
    public class MockData3 : LeafDataProtocol
    {
        int x = 5;
        public void Verify(Logger lg = null)
        {
            Assert.AreEqual(5, x);
            if (lg != null) lg.Log("data3");
        }
    }

    [Serializable]
    public class MockData4 : LeafDataProtocol
    {
        float f = 5.6f;
        public void Verify(Logger lg = null)
        {
            Assert.AreEqual(5.6, f);
            if (lg != null) lg.Log("data4");
        }
    }

    public class MockData1Handler : AbstractProtocolHandler<MockData1>
    {
        public event Action<MockData1> NewData;
        public void Send() => RequestSendToDownstream?.Invoke(new MockData1(), null);

        public override event Action<MockData1, AbstractProtocol> RequestSendToDownstream;

        public override MockData1 DownstreamProcess(object data) => new MockData1();
        public override void UpstreamProcess(MockData1 protocol)
        {
            protocol.Verify();
            NewData?.Invoke(protocol);
        }
    }

    public class MockData2Handler : AbstractProtocolHandler<MockData2>
    {
        public event Action<MockData2> NewData;
        public void Send() => RequestSendToDownstream?.Invoke(new MockData2(), null);

        public override event Action<MockData2, AbstractProtocol> RequestSendToDownstream;

        public override MockData2 DownstreamProcess(object data) => new MockData2();
        public override void UpstreamProcess(MockData2 protocol)
        {
            protocol.Verify();
            NewData?.Invoke(protocol);
        }
    }

    public class MockData3Handler : AbstractProtocolHandler<MockData3>
    {
        public event Action<MockData3> NewData;
        public void Send() => RequestSendToDownstream?.Invoke(new MockData3(), null);

        public override event Action<MockData3, AbstractProtocol> RequestSendToDownstream;

        public override MockData3 DownstreamProcess(object data) => new MockData3();
        public override void UpstreamProcess(MockData3 protocol)
        {
            protocol.Verify();
            NewData?.Invoke(protocol);
        }
    }
}
