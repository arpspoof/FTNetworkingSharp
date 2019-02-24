using System;
using System.IO;
using static Infra.Logger;

namespace Infra.DataService.Protocol
{
    public interface IDataSerializer : IDataEncapsulator<AbstractProtocol>, IDataSender<Stream> { }
    public interface IDataDeserializer : IDataReceiver<Stream>, IDataProvider<AbstractProtocol> { }

    public class DataSerializer<TFormatter> : IDataSerializer, IDataDeserializer, IDataErrorGenerator
        where TFormatter : System.Runtime.Serialization.IFormatter, new()
    {
        private TFormatter formatter = new TFormatter();

        private const int bufferSize = 409600;
        private int bufferPointer = 0, packetLength = 0;
        private readonly byte[] lengthBuffer = new byte[4];
        private readonly byte[] receiveBuffer = new byte[bufferSize];

        public event Action<Stream> SenderDataReady;
        public event Action<AbstractProtocol> ProviderDataReady;
        public event Action<DataIntegrityError> DataIntegrityError;
        public event Action<DataSymmetricityError> DataSymmetricityError;

        public void Encapsulate(AbstractProtocol data)
        {
            using (MemoryStream stream = new MemoryStream(), dataStream = new MemoryStream())
            {
                try
                {
                    formatter.Serialize(dataStream, data);
                }
                catch
                {
                    throw new DeveloperError("DataSerializer", "Encapsulate",
                        "Not serializable AbstractProtocol",
                        "Please mark all protocol as [Serializable]");
                }

                dataStream.Seek(0, SeekOrigin.Begin);
                stream.Write(BitConverter.GetBytes(dataStream.Length), 0, 4);
                dataStream.CopyTo(stream);

                stream.Seek(0, SeekOrigin.Begin);
                SenderDataReady?.Invoke(stream);

                dataStream.Close();
                stream.Close();
            }
        }

        public void Receive(Stream data) 
        {
            if (packetLength == 0)
            {
                data.Read(lengthBuffer, 0, 4);
                packetLength = BitConverter.ToInt32(lengthBuffer, 0);
                Log($"new data: packet length = {packetLength}", "DS");
                if (packetLength > bufferSize)
                {
                    Log($"DataIntegrityError: packet length = {packetLength}", "DS", LogType.ERROR);
                    DataIntegrityError?.Invoke(new DataIntegrityError("receive buffer overflow"));
                    return;
                }
                if (packetLength <= 0)
                {
                    Log($"DataIntegrityError: packet length = {packetLength}", "DS", LogType.ERROR);
                    DataIntegrityError?.Invoke(new DataIntegrityError("non-positive packet length"));
                    return;
                }
            }
            if (bufferPointer < packetLength)
            {
                int dataLength = data.Read(receiveBuffer, bufferPointer, packetLength - bufferPointer);
                bufferPointer += dataLength;
                Log($"buffering data: packet length = {packetLength}, bufferPointer = {bufferPointer}", "DS");
            }
            if (bufferPointer == packetLength)
            {
                using (MemoryStream copy = new MemoryStream(receiveBuffer, 0, bufferPointer))
                {
                    ClearBuffer();
                    object obj = null;
                    try
                    {
                        Log($"consuming data ...", "DS");
                        obj = formatter.Deserialize(copy);
                    }
                    catch
                    {
                        Log($"DataIntegrityError: non-serializable data", "DS", LogType.ERROR);
                        DataIntegrityError?.Invoke(
                            new DataIntegrityError("received non-serializable data"));
                        ClearBuffer();
                        return;
                    }

                    if (!(obj is AbstractProtocol))
                    {
                        Log($"DataIntegrityError: data is not [AbstractProtocol]", "DS", LogType.ERROR);
                        DataSymmetricityError?.Invoke(
                            new DataSymmetricityError("received data is not [AbstractProtocol]"));
                        ClearBuffer();
                        return;
                    }

                    ProviderDataReady?.Invoke((AbstractProtocol)obj);
                    copy.Close();
                }
            }
            if (data.Position < data.Length)
            {
                Log($"data is too large, position = {data.Position}, size = {data.Length}", "DS");
                Receive(data);
            }
        }

        public void ClearBuffer()
        {
            Log($"buffer is cleared", "DS");
            bufferPointer = packetLength = 0;
        }
    }

    public class DataSerializer : DataSerializer<System.Runtime.Serialization.Formatters.Binary.BinaryFormatter>
    {
    }
}
