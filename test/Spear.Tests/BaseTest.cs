using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Spear.Core;
using System;

namespace Spear.Tests
{
    public abstract class BaseTest
    {
        private readonly IServiceProvider _provider;

        protected BaseTest()
        {
            var services = new ServiceCollection();
            services.AddLogging(builder =>
            {
                builder.SetMinimumLevel(LogLevel.Debug);
                builder.AddConsole();
            });
            InitServices(services);
            _provider = services.BuildServiceProvider();
            InitUseServices(_provider);
        }

        private void InitUseServices(IServiceProvider provider)
        {
            UseServices(provider);
        }

        private void InitServices(IServiceCollection services)
        {
            MapServices(services);
        }

        protected virtual void MapServices(IServiceCollection services)
        {
        }

        protected virtual void UseServices(IServiceProvider provider)
        {

        }

        protected T Resolve<T>()
        {
            return _provider.GetService<T>();
        }

        protected void Print(object obj)
        {
            if (obj == null)
            {
                Console.WriteLine("NULL");
            }
            else if (obj.GetType().IsSimpleType())
            {
                if (obj is DateTime date)
                    Console.WriteLine($"{date:u}");
                else
                    Console.WriteLine(obj.ToString());
            }
            else
            {
                Console.WriteLine(JsonConvert.SerializeObject(obj, Formatting.Indented));
            }
        }
    }
}
