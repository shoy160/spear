using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace Spear.Core.Micro.Services
{
    public abstract class DServiceRegister : DServiceRoute, IServiceRegister
    {
        public abstract Task Regist(IEnumerable<Assembly> assemblyList, ServiceAddress serverAddress);

        public abstract Task Deregist();
    }
}
