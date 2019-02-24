using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Networking.Core.UnitTest
{
    [TestClass]
    public class ProtocolTreeTest
    {
        [TestMethod]
        public void ProtocolTree()
        {
            Logger lg = new Logger();
            ProtocolTree tree = new ProtocolTree();
            tree.AttachErrorHandler();
            MockData1Handler h1 = new MockData1Handler();
            MockData2Handler h2 = new MockData2Handler();
            MockData3Handler h3 = new MockData3Handler();
            LeafProtocolHandler<MockData4> h4 = new LeafProtocolHandler<MockData4>();
            LeafProtocolHandler<MockData4> h5 = new LeafProtocolHandler<MockData4>();

            tree.Register(h1);
            tree.Register(h2);
            tree.Register(h3);
            tree.Register(h4);
            tree.Register(h5);
            tree.Entry(h1);
            tree.Entry(h2);
            tree.ConnectToLeaf(h1, h4);
            tree.Connect(h2, h3);
            tree.EntryToLeaf(h5);
            tree.SenderDataReady += obj => tree.Receive(obj);

            h1.NewData += obj => lg.Log("data1");
            h2.NewData += obj => lg.Log("data2");
            h3.NewData += obj => lg.Log("data3");
            h4.NewData += obj => lg.Log("data4");
            h5.NewData += obj => lg.Log("data5");
            h1.Send();
            h2.Send();
            h3.Send();
            h4.Send(new MockData4());
            h5.Send(new MockData4());
            lg.Verify(new List<string>
            {
                "data1", "data2", "data2", "data3", "data1", "data4", "data5"
            });
        }

        [TestMethod]
        public void TreeMerge()
        {
            Logger lg = new Logger();
            ProtocolTree tree1 = new ProtocolTree();
            ProtocolTree tree2 = new ProtocolTree();
            ProtocolTree tree3 = new ProtocolTree();
            tree1.AttachErrorHandler();
            tree2.AttachErrorHandler();
            tree3.AttachErrorHandler();
            MockData1Handler h11 = new MockData1Handler();
            MockData2Handler h12 = new MockData2Handler();
            MockData3Handler h2 = new MockData3Handler();
            LeafProtocolHandler<MockData4> h31 = new LeafProtocolHandler<MockData4>();
            LeafProtocolHandler<MockData3> h32 = new LeafProtocolHandler<MockData3>();

            tree1.Register(h11);
            tree1.Register(h12);
            tree1.Entry(h11);
            tree1.Entry(h12);
            tree2.Register(h2);
            tree2.Entry(h2);
            tree1.Connect(h12, tree2);
            tree3.Register(h31);
            tree3.Register(h32);
            tree3.EntryToLeaf(h31);
            tree3.EntryToLeaf(h32);
            tree1.Entry(tree3);
            tree1.SenderDataReady += obj => tree1.Receive(obj);

            h11.NewData += obj => lg.Log("data11");
            h12.NewData += obj => lg.Log("data12");
            h2.NewData += obj => lg.Log("data2");
            h31.NewData += obj => lg.Log("data31");
            h32.NewData += obj => lg.Log("data32");
            h11.Send();
            h12.Send();
            h2.Send();
            h31.Send(new MockData4());
            h32.Send(new MockData3());
            lg.Verify(new List<string>
            {
                "data11", "data12", "data12", "data2", "data31", "data32"
            });
        }
    }

    [TestClass]
    public class ProtocolTreeErrorTest
    {
        [TestMethod]
        public void ProtocolTreeErrorUnregister()
        {
            ProtocolTree tree = new ProtocolTree();
            tree.AttachErrorHandler();
            MockData1Handler h1 = new MockData1Handler();
            MockData2Handler h2 = new MockData2Handler();
            Assert.ThrowsException<DeveloperError>(() => tree.Entry(h1));
            Assert.ThrowsException<DeveloperError>(() => tree.Connect(h1, h2));
        }

        [TestMethod]
        public void ProtocolTreeErrorSameTypeChildNodes()
        {
            ProtocolTree tree = new ProtocolTree();
            tree.AttachErrorHandler();
            MockData1Handler h1 = new MockData1Handler();
            MockData1Handler h2 = new MockData1Handler();
            tree.Register(h1);
            tree.Register(h2);
            tree.Entry(h1);
            Assert.ThrowsException<DeveloperError>(() => tree.Entry(h2));
        }

        [TestMethod]
        public void ProtocolTreeErrorMismatch()
        {
            ProtocolTree tree1 = new ProtocolTree();
            ProtocolTree tree2 = new ProtocolTree();
            tree1.AttachErrorHandler();
            tree2.AttachErrorHandler();
            MockData1Handler h11 = new MockData1Handler();
            MockData2Handler h21 = new MockData2Handler();
            tree1.Register(h11);
            tree2.Register(h21);
            tree1.Entry(h11);
            tree2.Entry(h21);
            tree1.SenderDataReady += obj =>
            {
                Assert.ThrowsException<DataSymmetricityError>(() => tree2.Receive(obj));
            };
            h11.Send();
        }

        [TestMethod]
        public void ProtocolTreeErrorMismatchWithMerge()
        {
            ProtocolTree tree1 = new ProtocolTree();
            ProtocolTree tree2 = new ProtocolTree();
            tree1.AttachErrorHandler();
            MockData1Handler h11 = new MockData1Handler();
            MockData2Handler h21 = new MockData2Handler();
            tree1.Register(h11);
            tree2.Register(h21);
            tree1.Entry(h11);
            tree2.Entry(h21);
            tree1.Connect(h11, tree2);

            ProtocolTree tree3 = new ProtocolTree();
            ProtocolTree tree4 = new ProtocolTree();
            tree3.AttachErrorHandler();
            MockData1Handler h31 = new MockData1Handler();
            MockData3Handler h41 = new MockData3Handler();
            tree3.Register(h31);
            tree4.Register(h41);
            tree3.Entry(h31);
            tree4.Entry(h41);
            tree3.Connect(h31, tree4);

            tree1.SenderDataReady += obj =>
            {
                Assert.ThrowsException<DataSymmetricityError>(() => tree3.Receive(obj));
            };
            h21.Send();
        }

        [TestMethod]
        public void ProtocolTreeErrorGap()
        {
            Logger lg = new Logger();
            ProtocolTree tree1 = new ProtocolTree();
            ProtocolTree tree2 = new ProtocolTree();
            tree1.AttachErrorHandler();
            tree2.AttachErrorHandler();
            MockData1Handler h11 = new MockData1Handler();
            MockData2Handler h12 = new MockData2Handler();
            MockData1Handler h21 = new MockData1Handler();
            h21.NewData += obj => lg.Log("data1");
            tree1.Register(h11);
            tree1.Register(h12);
            tree2.Register(h21);
            tree1.Entry(h11);
            tree1.Connect(h11, h12);
            tree2.Entry(h21);
            void test1(AbstractProtocol obj)
            {
                tree2.Receive(obj);
            }
            void test2(AbstractProtocol obj)
            {
                Assert.ThrowsException<DataSymmetricityError>(() => tree2.Receive(obj));
            }
            tree1.SenderDataReady += test1;
            h11.Send();
            tree1.SenderDataReady -= test1;
            tree1.SenderDataReady += test2;
            h12.Send();
            lg.Verify(new List<string>
            {
                "data1", "data1"
            });
        }
    }
}
