using System;
using System.IO;

namespace Infra.DataService.Networking.UnitTest
{
    public class MockWire
    {
        public event Action Connect, Disconnect;

        public bool Connected { get; private set; }

        public void Up() { Connected = true; Connect?.Invoke(); }
        public void Down() { Connected = false; Disconnect?.Invoke(); }
        public void SilentDown() { Connected = false; }
    }

    public class MockTCP : ITransportationLayer
    {
        private MockWire wire;

        public event Action ConnectionEstablished;
        public event Action ConnectionLost;
        public event Action<Stream> ProviderDataReady;

        public MockTCP other { get; set; }

        public MockTCP(MockWire wire)
        {
            this.wire = wire;
            wire.Connect += () => ConnectionEstablished?.Invoke();
            wire.Disconnect += () => ConnectionLost?.Invoke();
        }

        public void Encapsulate(Stream data)
        {
            if (!wire.Connected) return;
            Stream rs = new MemoryStream();
            data.CopyTo(rs);
            rs.Seek(0, SeekOrigin.Begin);
            other.ProviderDataReady?.Invoke(rs);
            rs.Close();
        }

        public void CloseConnection()
        {
            throw new NotImplementedException();
        }
    }
}
