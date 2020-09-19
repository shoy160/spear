using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Spear.Core.Config;
using System;
using System.Threading.Tasks;

namespace Spear.WebApi
{
    public class DHost : DHost<DStartup>
    {
    }


    public class DHost<TStart> where TStart : DStartup
    {
        /// <summary> 宿主机构建器 </summary>
        protected static event Action<IHostBuilder> Builder;

        /// <summary> 配置构建 </summary>
        protected static event Action<ISpearConfigBuilder> ConfigBuilder;

        /// <summary> 开启服务 </summary>
        /// <param name="args"></param>
        public static async Task Start(string[] args)
        {
            var hostBuilder = Host.CreateDefaultBuilder(args)
                .UseServiceProviderFactory(new AutofacServiceProviderFactory())
                .ConfigureAppConfiguration((context, cb) =>
                {
                    var builder = cb.AddSpearConfig();
                    ConfigBuilder?.Invoke(builder);
                    context.Configuration = builder.Build();
                });
            if (Builder == null)
            {
                UseKestrel(hostBuilder);
            }
            else
            {
                Builder?.Invoke(hostBuilder);
            }
            var host = hostBuilder.Build();
            await host.RunAsync();
        }

        /// <summary> 使用Kestrel作为宿主服务 </summary>
        /// <param name="builder"></param>
        /// <param name="optionAction"></param>
        protected static void UseKestrel(IHostBuilder builder, Action<KestrelServerOptions> optionAction = null)
        {
            builder
                .ConfigureWebHostDefaults(b =>
                {
                    b.UseKestrel(options =>
                        {
                            options.AllowSynchronousIO = true;
                            options.Limits.RequestHeadersTimeout = TimeSpan.FromSeconds(65);
                            optionAction?.Invoke(options);
                        })
                        .UseStartup<TStart>();
                })
                .ConfigureServices((context, services) =>
                {
                    var section = context.Configuration.GetSection("Kestrel");
                    if (section != null)
                        services.Configure<KestrelServerOptions>(section);
                });
        }

        /// <summary> 使用IIS作为宿主服务 </summary>
        /// <param name="builder"></param>
        protected static void UseIIS(IHostBuilder builder)
        {
            builder.ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder
                    .UseIISIntegration()
                    .UseStartup<TStart>();
            });
        }
    }
}
