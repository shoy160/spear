using Microsoft.Extensions.Logging;
using Spear.Core.Extensions;
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
        private readonly ConcurrentDictionary<string, MicroEntry> _entries;
        protected readonly ITypeFinder TypeFinder;
        private readonly IServiceProvider _provider;

        public MicroEntryFactory(ILogger<MicroEntryFactory> logger, ITypeFinder typeFinder, IServiceProvider provider)
        {
            _entries = new ConcurrentDictionary<string, MicroEntry>();
            Services = new List<Type>();
            _logger = logger;
            TypeFinder = typeFinder;
            _provider = provider;
            InitServices();
        }

        private bool HasImpl(Type interfaceType)
        {
            return TypeFinder.Find(t => interfaceType.IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract).Any();
        }

        /// <summary> 初始化服务 </summary>
        private void InitServices()
        {
            var services = FindServices();
            foreach (var service in services)
            {
                if (!HasImpl(service))
                    continue;
                Services.Add(service);
                var methods = service.GetMethods(BindingFlags.Public | BindingFlags.Instance);
                foreach (var method in methods)
                {
                    if (FilterMethod(method))
                        continue;
                    var serviceId = GenerateServiceId(method);
                    _entries.TryAdd(serviceId, CreateEntry(method));
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

        protected virtual List<Type> FindServices()
        {
            return TypeFinder
                .Find(t => typeof(ISpearService).IsAssignableFrom(t) && t.IsInterface && t != typeof(ISpearService))
                .ToList();
        }

        protected virtual bool FilterMethod(MethodInfo method)
        {
            return false;
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
                if (_logger.IsEnabled(LogLevel.Warning))
                    _logger.LogWarning("方法的定义类型不能为空。");
                throw new ArgumentNullException(nameof(method.DeclaringType), "方法的定义类型不能为空。");
            }

            var id = method.ServiceKey();
            if (_logger.IsEnabled(LogLevel.Debug))
                _logger.LogDebug($"为方法：{method}生成服务Id：{id}。");
            return id;
        }

        public List<Type> Services { get; }

        /// <summary> 获取服务列表 </summary>
        /// <returns></returns>
        public virtual IEnumerable<Assembly> GetContracts()
        {
            var list = new List<Assembly>();
            foreach (var service in Services)
            {
                var ass = service.Assembly;
                if (list.Contains(ass))
                    continue;
                list.Add(ass);
            }

            return list;
        }

        /// <summary> 服务方法 </summary>
        public IDictionary<string, MicroEntry> Entries => _entries;

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
            if (_entries.TryGetValue(serviceId, out var method))
                return method;
            throw new ArgumentNullException(nameof(serviceId), "服务条目未找到");
        }
    }
}
