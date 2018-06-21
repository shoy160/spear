using System;
using System.Collections.Generic;

namespace Spear.Core.Micro.Services
{
    public interface IServiceFinder
    {
        IEnumerable<string> Find(Type serviceType);
    }
}
