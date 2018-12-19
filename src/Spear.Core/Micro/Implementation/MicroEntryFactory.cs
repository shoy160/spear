using Acb.Core;
using Acb.Core.Dependency;
using Acb.Core.Logging;
using Acb.Core.Reflection;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Spear.Core.Micro.Implementation
{
    /// <summary>/// 本地服务工厂
    /// </summary>
    public class MicroEntryFactory : IMicroEntryFactory
    {
        private readonly ILogger _logger;
        private readonly ITypeFinder _typeFinder;
        private readonly ConcurrentDictionary<string, MethodInfo> _methods;

        public MicroEntryFactory()
        {
            _logger = LogManager.Logger<MicroEntryFactory>();
            _methods = new ConcurrentDictionary<string, MethodInfo>();
            _typeFinder = CurrentIocManager.Resolve<ITypeFinder>();
            InitServices();
        }

        /// <summary> 初始化服务 </summary>
        private void InitServices()
        {
            if (_typeFinder == null) return;
            var services = _typeFinder
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
            var id = method.ServiceKey();
            _logger.Debug($"为方法：{method}生成服务Id：{id}。");
            return id;
        }

        public IEnumerable<Assembly> GetServices()
        {
            var list = new List<Assembly>();
            foreach (var methodInfo in _methods.Values)
            {
                var ass = methodInfo.DeclaringType?.Assembly;
                if (ass == null || list.Contains(ass))
                    continue;
                list.Add(ass);
            }

            return list;
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
