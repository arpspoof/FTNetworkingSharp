using Infra.DataService.Protocol;

namespace Infra.ServiceFramework.Host
{
    public interface IHostControl
    {
        bool OpenInterface(int index);
        bool CloseInterface(int index);
    }
}
