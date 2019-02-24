using System;
using System.IO;
using Infra.DataService.Protocol;

namespace Infra.DataService.IO
{
    public abstract class AbstractIODevice :
        IDataEncapsulator<Stream>, IDataProvider<Stream>
    {
        public abstract event Action<Stream> ProviderDataReady;
        public abstract void Encapsulate(Stream data);
        public abstract void Read();
    }
}
