using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Spear.Core;
using Spear.Core.Micro.Implementation;
using Spear.Core.Reflection;

namespace Spear.Protocol.Grpc
{
    public class GrpcEntryFactory : MicroEntryFactory
    {
        public GrpcEntryFactory(ILogger<MicroEntryFactory> logger, ITypeFinder typeFinder, IServiceProvider provider) :
            base(logger, typeFinder, provider)
        {
        }

        protected override List<Type> FindServices()
        {
            var types = TypeFinder
                .Find(t => typeof(ISpearService).IsAssignableFrom(t) && t.IsClass && !t.IsAbstract)
                .ToList();

            return types;
        }

        protected override bool FilterMethod(MethodInfo method)
        {
            return method != null && method.ReturnType.BaseType != typeof(Task);
        }

        public override IEnumerable<Assembly> GetContracts()
        {
            var list = new List<Assembly>();
            foreach (var service in Services)
            {
                var ass = service.BaseType?.Assembly ?? service.Assembly;
                if (list.Contains(ass))
                    continue;
                list.Add(ass);
            }

            return list;
        }
    }
}
