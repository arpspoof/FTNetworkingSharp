using System.Collections.Generic;
using Infra.DataService.Protocol;

namespace Infra.DataService.IO
{
    public interface IAbstractIO
    {
        void Write(AbstractProtocol protocol);
        List<AbstractProtocol> Read();
    }
}
