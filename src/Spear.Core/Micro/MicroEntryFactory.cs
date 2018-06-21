using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using Acb.Core;
using Acb.Core.Dependency;
using Acb.Core.Logging;
using Acb.Core.Reflection;

namespace Spear.Core.Micro
{
    public class MicroEntryFactory : IMicroEntryFactory
    {
        private readonly ILogger _logger;
        private readonly ConcurrentDictionary<string, MethodInfo> _methods;

        public MicroEntryFactory()
        {
            _logger = LogManager.Logger<MicroEntryFactory>();
            _methods = new ConcurrentDictionary<string, MethodInfo>();
            InitServices();
        }

        /// <summary> 初始化服务 </summary>
        private void InitServices()
        {
            var finder = CurrentIocManager.Resolve<ITypeFinder>();
            if (finder == null) return;
            var services = finder
                .Find(t => typeof(IMicroService).IsAssignableFrom(t) && t.IsInterface && t != typeof(IMicroService))
                .ToList();
            foreach (var service in services)
            {
                if (!CurrentIocManager.IsRegisted(service))
                    continue;
                var methods = service.GetMethods(BindingFlags.Public | BindingFlags.Instance);
                foreach (var method in methods)
                {
                    var serviceId = GenerateServiceId(method);
                    _methods.TryAdd(serviceId, method);
                }
            }
        }

        protected virtual string GenerateServiceId(MethodInfo method)
        {
            if (method == null)
                throw new ArgumentNullException(nameof(method));
            var type = method.DeclaringType;
            if (type == null)
                throw new ArgumentNullException(nameof(method.DeclaringType), "方法的定义类型不能为空。");

            var id = $"{type.FullName}.{method.Name}";
            var parameters = method.GetParameters();
            if (parameters.Any())
            {
                id += "_" + string.Join("_", parameters.Select(i => i.Name));
            }
            _logger.Debug($"为方法：{method}生成服务Id：{id}。");
            return id;
        }


        public string GetServiceId(MethodInfo method)
        {
            return GenerateServiceId(method);
        }

        public MethodInfo Find(string serviceId)
        {
            if (_methods.TryGetValue(serviceId, out var method))
                return method;
            throw new ArgumentNullException(nameof(serviceId), "服务条目未找到");
        }
    }
}
