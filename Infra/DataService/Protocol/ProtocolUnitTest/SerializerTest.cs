using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Infra.DataService.Protocol.UnitTest
{
    internal class SerializerTestData : AbstractProtocol { public int x = 6; }

    [TestClass]
    public class SerializerTest
    {
        [TestMethod]
        public void Serializer()
        {
            DataSerializer s = new DataSerializer();
            s.AttachErrorHandler();
            s.SenderDataReady += stream =>
            {
                s.ProviderDataReady += obj => ((MockData1)obj).Verify();
                s.Receive(stream);
            };
            s.Encapsulate(new MockData1());
        }

        [TestMethod]
        public void DataConcatenation()
        {
            DataSerializer s = new DataSerializer();
            s.AttachErrorHandler();
            s.SenderDataReady += data =>
            {
                MemoryStream ms = (MemoryStream)data;
                ms.Seek(0, SeekOrigin.Begin);
                byte[] ar = ms.ToArray();

                MemoryStream stream1 = new MemoryStream(ar, 0, 16);
                MemoryStream stream2 = new MemoryStream(ar, 16, ar.Length - 16);

                s.ProviderDataReady += obj => ((MockData1)obj).Verify();
                s.Receive(stream1);
                s.Receive(stream2);
            };

            s.Encapsulate(new MockData1());
        }
    }

    [TestClass]
    public class SerializerErrorTest
    {
        [TestMethod]
        public void SerializerError()
        {
            DataSerializer s = new DataSerializer();
            s.AttachErrorHandler();
            Assert.ThrowsException<DeveloperError>(() => s.Encapsulate(new SerializerTestData()));
        }

        [TestMethod]
        public void DeserializerMismatchError()
        {
            DataSerializer s = new DataSerializer();
            s.AttachErrorHandler();

            Assert.ThrowsException<DeveloperError>(() => s.Encapsulate(new SerializerTestData()));

            BinaryFormatter formatter = new BinaryFormatter();

            MemoryStream stream2 = new MemoryStream();
            formatter.Serialize(stream2, 5);

            byte[] ar = stream2.ToArray();
            MemoryStream stream3 = new MemoryStream();
            stream3.Write(BitConverter.GetBytes(ar.Length), 0, 4);

            stream2.Seek(0, SeekOrigin.Begin);
            stream2.CopyTo(stream3);

            stream3.Seek(0, SeekOrigin.Begin);
            Assert.ThrowsException<DataSymmetricityError>(() => s.Receive(stream3));
        }

        [TestMethod]
        public void DeserializerNotSerializableError()
        {
            DataSerializer s = new DataSerializer();
            s.AttachErrorHandler();

            BinaryFormatter formatter = new BinaryFormatter();

            MemoryStream stream = new MemoryStream();
            formatter.Serialize(stream, new byte[1200]);

            stream.Seek(0, SeekOrigin.Begin);
            Assert.ThrowsException<DataIntegrityError>(() => s.Receive(stream));
        }
    }
}
