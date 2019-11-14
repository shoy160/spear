using Spear.Core.Micro.Services;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace Spear.Nacos
{
    public class NacosServiceRegister : IServiceRegister
    {
        public Task Regist(IEnumerable<Assembly> assemblyList, ServiceAddress serverAddress)
        {
            throw new System.NotImplementedException();
        }

        public Task Deregist()
        {
            throw new System.NotImplementedException();
        }
    }
}
