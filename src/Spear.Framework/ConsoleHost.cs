using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Spear.Core.Config;
using Spear.Core.Extensions;
using Spear.Core.Serialize;
using System;
using System.Text;

namespace Spear.Framework
{
    /// <summary> 控制台宿主机 </summary>
    public class ConsoleHost
    {
        /// <summary> 命令事件 </summary>
        protected static event Action<string, IContainer> Command;
        /// <summary> 注册服务 </summary>
        protected static event Action<IServiceCollection> MapServiceCollection;
        /// <summary> 注册服务 </summary>
        protected static event Action<ContainerBuilder> MapServices;

        /// <summary> 使用服务 </summary>
        protected static event Action<IServiceProvider> UseServiceProvider;
        /// <summary> 使用服务 </summary>
        protected static event Action<IContainer> UseServices;

        /// <summary> 配置构建 </summary>
        protected static event Action<ISpearConfigBuilder> ConfigBuild;

        /// <summary> 停止事件 </summary>
        protected static event Action StopEvent;

        /// <summary> 启动项 </summary>
        protected static SpearBootstrap Bootstrap { get; private set; }

        /// <summary> 开启服务 </summary>
        /// <param name="args"></param>
        protected static void Start(string[] args)
        {
            Bootstrap = new SpearBootstrap();
            var services = new ServiceCollection();
            services.AddSpearConfig(ConfigBuild);

            services.AddLogging(builder =>
            {
                builder.AddConfiguration(ConfigHelper.Instance.Config.GetSection("Logging"));
                builder.AddConsole();
            });

            MapServiceCollection?.Invoke(services);
            Bootstrap.BuilderHandler += b =>
            {
                b.Populate(services);
                MapServices?.Invoke(b);
            };
            Bootstrap.Initialize();
            var container = Bootstrap.CreateContainer();
            if (UseServiceProvider != null)
            {
                var provider = new AutofacServiceProvider(container);
                UseServiceProvider.Invoke(provider);
            }

            UseServices?.Invoke(container);
            AppDomain.CurrentDomain.ProcessExit += (sender, e) =>
            {
                Shutdown();
            };
            Console.CancelKeyPress += (sender, e) =>
            {
                Shutdown();
            };
            while (true)
            {
                var cmd = Console.ReadLine();
                if (cmd == "exit")
                    break;
                Command?.Invoke(cmd, container);
            }
        }

        private static void Shutdown()
        {
            StopEvent?.Invoke();
            Bootstrap.Dispose();
        }

        protected static T Resolve<T>()
        {
            return Bootstrap.IocManager.Resolve<T>();
        }

        /// <summary> 打印数据 </summary>
        /// <param name="result"></param>
        /// <param name="times">耗时</param>
        protected static void Print(object result, long? times = null)
        {
            var msg = new StringBuilder();
            if (result == null)
            {
                msg.Append("NULL");
            }
            else
            {
                var type = result.GetType();
                if (type.IsSimpleType())
                    msg.Append(result);
                else
                {
                    try
                    {
                        msg.Append(JsonHelper.ToJson(result, NamingType.CamelCase, true));
                    }
                    catch
                    {
                        msg.Append(result);
                    }
                }
            }

            if (times.HasValue)
                msg.Append($" -> time:{times}ms");
            Console.WriteLine(msg);
        }
    }
}
