using Infra.DataService.Protocol;

namespace Infra.ServiceFramework.Host
{
    public interface IServiceLogic
    {
        int MaxConnection { get; }
        IHostControl Host { set; }
        ProtocolTree GetProtocolTree(int index);
        void Init();
        void OnClientEnter(int index);
        void OnClientLeave(int index);
        bool CanResume(int index);
        void OnClientDisconnect(int index);
        void OnClientResume(int index);
    }
}
