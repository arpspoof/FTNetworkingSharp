using Infra.DataService.Protocol;

namespace Infra.ServiceFramework.Client
{
    public interface IClientLogic
    {
        ProtocolTree ProtocolTree { get; }
    }
}
