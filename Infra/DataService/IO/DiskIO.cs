using System;
using System.IO;

namespace Infra.DataService.IO
{
    public class DiskIO : AbstractIODevice
    {
        private readonly string filePath;

        private const int maxReadSize = 81920;
        private readonly byte[] readBuffer = new byte[maxReadSize];

        public DiskIO(string path) => filePath = path;

        public override event Action<Stream> ProviderDataReady;

        public override void Encapsulate(Stream data)
        {
            var dataStream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
            data.CopyTo(dataStream);
            dataStream.Close();
        }

        public override void Read()
        {
            var dataStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            while (dataStream.Position < dataStream.Length)
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    int nRead = dataStream.Read(readBuffer, 0, maxReadSize);
                    stream.Write(readBuffer, 0, nRead);
                    stream.Seek(0, SeekOrigin.Begin);
                    ProviderDataReady?.Invoke(stream);
                    stream.Close();
                }
            }
            dataStream.Close();
        }
    }
}
