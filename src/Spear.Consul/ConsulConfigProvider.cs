using Microsoft.Extensions.Configuration;
using Spear.Core.Config;
using System;

namespace Spear.Consul
{
    public class ConsulConfigProvider : DConfigProvider, IConfigurationSource
    {
        public override void Reload(object state = null)
        {
            throw new NotImplementedException();
        }

        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            throw new NotImplementedException();
        }
    }
}
