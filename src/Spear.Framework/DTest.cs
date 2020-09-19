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
    /// <summary> 测试基类 </summary>
    public abstract class DTest
    {
        /// <summary> 项目启动项 </summary>
        protected SpearBootstrap Bootstrap;

        protected virtual void MapServices(ContainerBuilder builder)
        {
        }

        protected virtual void MapServices(IServiceCollection services)
        {
        }

        protected virtual void UseServices(IServiceProvider provider)
        {
        }

        protected virtual void ConfigBuilder(ISpearConfigBuilder builder)
        {
        }

        /// <summary> 默认构造函数 </summary>
        protected DTest()
        {
            Init();
        }

        private void Init()
        {
            var services = new ServiceCollection();
            services
                .AddSpearConfig(ConfigBuilder)
                .AddLogging(builder =>
            {
                builder.AddConfiguration(ConfigHelper.Instance.Config.GetSection("Logging"));
                builder.AddConsole();
            });

            MapServices(services);
            Bootstrap = new SpearBootstrap();
            Bootstrap.BuilderHandler += builder =>
            {
                builder.Populate(services);
                MapServices(builder);
            };
            Bootstrap.Initialize();
            var container = Bootstrap.CreateContainer();
            var provider = new AutofacServiceProvider(container);
            UseServices(provider);
        }

        protected T Resolve<T>()
        {
            return Bootstrap.ContainerRoot.Resolve<T>();
        }

        /// <summary> 打印数据 </summary>
        /// <param name="result"></param>
        /// <param name="times">耗时</param>
        protected void Print(object result, long? times = null)
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
