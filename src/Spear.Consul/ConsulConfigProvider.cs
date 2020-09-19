using Microsoft.Extensions.Configuration;
using Spear.Core.Config;
using System;
using System.Threading.Tasks;

namespace Spear.Consul
{
    public class ConsulConfigProvider : DConfigProvider, IConfigurationSource
    {
        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            throw new NotImplementedException();
        }

        protected override Task LoadConfig(bool reload = false)
        {
            throw new NotImplementedException();
        }
    }
}
