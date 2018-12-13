using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Spear.Core.Micro.Services
{
    public interface IServiceFinder
    {
        Task<List<ServiceAddress>> Find(Type serviceType);
    }
}
