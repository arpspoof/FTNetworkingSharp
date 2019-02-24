using System;
using System.Timers;
using Infra.DataService.Protocol;

namespace Infra.DataService.Networking
{
    public abstract partial class FTConnectionController 
        : AbstractProtocolHandler<FTConnectionProtocolHeader>, IDisposable
    {
#if DEBUG1
        public readonly _MockTimer heartBeatTimer = new _MockTimer();
        public readonly _MockTimer silenceTimer = new _MockTimer();
#else
        internal readonly Timer heartBeatTimer = new Timer();
        internal readonly Timer silenceTimer = new Timer();
#endif
        private int heartBeatTimeInterval;
        public int HeartBeatTimeInterval
        {
            get => heartBeatTimeInterval;
            set { heartBeatTimeInterval = value; heartBeatTimer.Interval = value; }
        }

        private int silenceTimeLimit;
        public int SilenceTimeLimit
        {
            get => silenceTimeLimit;
            set { silenceTimeLimit = value; silenceTimer.Interval = value; }
        }

        public event Action ConnectionRefused;
        public event Action ConnectionLost;

        public override event Action<FTConnectionProtocolHeader, AbstractProtocol> RequestSendToDownstream;

        private void Init(ITransportationStateProvider transportationStateProvider, 
            int heartBeatTimeInterval, int silenceTimeLimit)
        {
            HeartBeatTimeInterval = heartBeatTimeInterval;
            SilenceTimeLimit = silenceTimeLimit;

            heartBeatTimer.Elapsed += (source, e) => SendInternalData(FTConnectionProtocolHeader.HTB);
            silenceTimer.Elapsed += (source, e) =>
            {
                Logger.Log("connection time out", "FT", Logger.LogType.ERROR);
                silenceTimeoutTrigger.Fire();
            };

            BuildStateMachine(transportationStateProvider);
        }

        private void ResetHeartBeat() { heartBeatTimer.Stop(); heartBeatTimer.Start(); }
        private void ResetSilence() { silenceTimer.Stop(); silenceTimer.Start(); }

        protected void SendInternalData(FTConnectionProtocolHeader header, AbstractProtocol data = null)
        {
            Logger.Log($"Internal: REC={header.FlagREC}, HTB={header.FlagHTB}, state={StateDescr()}", "FT");
            if (stateMachine.ActiveStateId != 0)
            {
                RequestSendToDownstream?.Invoke(header, data);
                ResetHeartBeat();
            }
            else
            {
                Logger.Log($"force timer to stop", "FT");
                heartBeatTimer.Stop();
                silenceTimer.Stop();
            }
        }

        public override void UpstreamProcess(FTConnectionProtocolHeader protocol)
        {
            ResetSilence();
            Logger.Log($"Receive: REC={protocol.FlagREC}, HTB={protocol.FlagHTB}, state={StateDescr()}", "FT");
            if (protocol.FlagHTB) return;
            if (protocol.FlagRST) { ConnectionRefused?.Invoke(); return; }
            if (protocol.FlagREC) recoveryTrigger.Fire();
        }

        public override FTConnectionProtocolHeader DownstreamProcess(object data)
        {
            ResetHeartBeat();
            return FTConnectionProtocolHeader.DAT;
        }

        public void ResetConnection() => SendInternalData(FTConnectionProtocolHeader.RST);

        public void Dispose()
        {
            Logger.Log($"FT disposing, state={StateDescr()}", "FT");
            heartBeatTimer.Dispose();
            silenceTimer.Dispose();
        }
    }
}
