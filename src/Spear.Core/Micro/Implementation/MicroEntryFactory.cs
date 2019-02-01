using Microsoft.Extensions.Logging;
using Spear.Core.Reflection;
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
        private readonly ILogger<MicroEntryFactory> _logger;
        private readonly ConcurrentDictionary<string, MethodInfo> _methods;
        private readonly ITypeFinder _typeFinder;

        public MicroEntryFactory(ILogger<MicroEntryFactory> logger, ITypeFinder typeFinder)
        {
            _methods = new ConcurrentDictionary<string, MethodInfo>();
            _logger = logger;
            _typeFinder = typeFinder;
            InitServices();
        }

        private bool HasImpl(Type interfaceType)
        {
            return _typeFinder.Find(t => interfaceType.IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract).Any();
        }

        /// <summary> 初始化服务 </summary>
        private void InitServices()
        {
            var services = _typeFinder
                .Find(t => typeof(ISpearService).IsAssignableFrom(t) && t.IsInterface && t != typeof(ISpearService))
                .ToList();
            foreach (var service in services)
            {
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
            {
                _logger.LogWarning("方法的定义类型不能为空。");
                throw new ArgumentNullException(nameof(method.DeclaringType), "方法的定义类型不能为空。");
            }

            var id = method.ServiceKey();
            _logger.LogDebug($"为方法：{method}生成服务Id：{id}。");
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
