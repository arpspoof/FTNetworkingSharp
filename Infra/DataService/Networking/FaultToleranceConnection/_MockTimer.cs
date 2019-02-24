using System;

namespace Infra.DataService.Networking
{
#if DEBUG
    public class _MockTimer : IDisposable
    {
        public int Interval { get; set; } // not used

        public bool Enabled { get; private set; }
        public event Action<int, int> Elapsed;

        public void Stop() => Enabled = false;
        public void Start() => Enabled = true;

        public void Tick()
        {
            if (Enabled) Elapsed?.Invoke(0, 0);
        }
        public void Dispose() { }
    }
#endif
}
