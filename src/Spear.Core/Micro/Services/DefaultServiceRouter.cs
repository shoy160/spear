using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace Spear.Core.Micro.Services
{
    public class DefaultServiceRouter : DServiceFinder, IServiceRegister
    {
        private readonly Dictionary<string, List<ServiceAddress>> serviceCenter;
        public DefaultServiceRouter(ILogger logger)
            : base(null, logger)
        {
            serviceCenter = new Dictionary<string, List<ServiceAddress>>();
        }

        public Task Deregist()
        {
            serviceCenter.Clear();
            return Task.CompletedTask;
        }

        public void Regist(string serviceName, ServiceAddress address)
        {
            Logger?.LogInformation($"regist service:{serviceName},{address}");
            if (!serviceCenter.TryGetValue(serviceName, out var list))
            {
                list = new List<ServiceAddress>();
            }
            list.Add(address);
            serviceCenter[serviceName] = list;
        }

        public Task Regist(IEnumerable<Assembly> assemblyList, ServiceAddress serverAddress)
        {
            foreach (var assembly in assemblyList)
            {
                var serviveName = assembly.ServiceName();
                Regist(serviveName, serverAddress);
            }
            return Task.CompletedTask;
        }

        protected override Task<List<ServiceAddress>> QueryService(Type serviceType, ProductMode[] modes)
        {
            var serviceName = serviceType.Assembly.ServiceName();
            if (!serviceCenter.TryGetValue(serviceName, out var list))
            {
                list = new List<ServiceAddress>();
            }
            return Task.FromResult(list);

        }
    }
}
