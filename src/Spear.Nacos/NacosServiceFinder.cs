using Spear.Core.Micro.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Spear.Nacos
{
    public class NacosServiceFinder : IServiceFinder
    {
        public Task<List<ServiceAddress>> Find(Type serviceType)
        {
            throw new NotImplementedException();
        }
    }
}
