using Microsoft.Extensions.Logging;
using Spear.Core.Reflection;
using Spear.ProxyGenerator;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Spear.Core.Micro.Implementation
{
    /// <summary>/// 本地服务工厂
    /// </summary>
    public class MicroEntryFactory : IMicroEntryFactory
    {
        private readonly ILogger<MicroEntryFactory> _logger;
        private readonly ConcurrentDictionary<string, MicroEntry> _services;
        private readonly ITypeFinder _typeFinder;
        private readonly IServiceProvider _provider;

        public MicroEntryFactory(ILogger<MicroEntryFactory> logger, ITypeFinder typeFinder, IServiceProvider provider)
        {
            _services = new ConcurrentDictionary<string, MicroEntry>();
            _logger = logger;
            _typeFinder = typeFinder;
            _provider = provider;
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
                if (!HasImpl(service))
                    continue;
                var methods = service.GetMethods(BindingFlags.Public | BindingFlags.Instance);
                foreach (var method in methods)
                {
                    var serviceId = GenerateServiceId(method);
                    _services.TryAdd(serviceId, CreateEntry(method));
                }
            }
        }

        private MicroEntry CreateEntry(MethodInfo method)
        {
            var fastInvoke = FastInvoke.GetMethodInvoker(method);

            return new MicroEntry(method)
            {
                Invoke = param =>
                {
                    var instance = _provider.GetService(method.DeclaringType);
                    var args = new List<object>();
                    var parameters = param ?? new Dictionary<string, object>();
                    foreach (var parameter in method.GetParameters())
                    {
                        if (parameters.ContainsKey(parameter.Name))
                        {
                            var parameterType = parameter.ParameterType;
                            var arg = parameters[parameter.Name].CastTo(parameterType);
                            args.Add(arg);
                        }
                        else if (parameter.HasDefaultValue)
                        {
                            args.Add(parameter.DefaultValue);
                        }
                    }
                    return Task.FromResult(fastInvoke(instance, args.ToArray()));
                }
            };
        }

        /// <summary> 生成服务ID </summary>
        /// <param name="method"></param>
        /// <returns></returns>
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

        /// <summary> 获取服务列表 </summary>
        /// <returns></returns>
        public IEnumerable<Assembly> GetContracts()
        {
            var list = new List<Assembly>();
            foreach (var methodInfo in _services.Values)
            {
                var ass = methodInfo.Method.DeclaringType?.Assembly;
                if (ass == null || list.Contains(ass))
                    continue;
                list.Add(ass);
            }

            return list;
        }

        /// <summary> 服务方法 </summary>
        public IDictionary<string, MicroEntry> Services => _services;

        /// <summary> 获取服务ID </summary>
        /// <param name="method"></param>
        /// <returns></returns>
        public string GetServiceId(MethodInfo method)
        {
            return GenerateServiceId(method);
        }

        /// <summary> 查找服务 </summary>
        /// <param name="serviceId"></param>
        /// <returns></returns>
        public MicroEntry Find(string serviceId)
        {
            if (_services.TryGetValue(serviceId, out var method))
                return method;
            throw new ArgumentNullException(nameof(serviceId), "服务条目未找到");
        }
    }
}
