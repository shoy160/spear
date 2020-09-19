using System.IO;
using Spear.Core.Config;
using Microsoft.Extensions.Configuration;

namespace Spear.Core.Extensions
{
    /// <summary> 配置扩展 </summary>
    public static class ConfigurationExtensions
    {
        /// <summary> 添加Json配置 </summary>
        /// <param name="builder"></param>
        /// <param name="json"></param>
        /// <returns></returns>
        public static IConfigurationBuilder AddJson(this IConfigurationBuilder builder, string json)
        {
            var parser = new JsonConfigurationParser();
            return builder.AddInMemoryCollection(parser.Parse(json));
        }

        /// <summary> 添加Json配置 </summary>
        /// <param name="builder"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        public static IConfigurationBuilder AddJson(this IConfigurationBuilder builder, Stream input)
        {
            var parser = new JsonConfigurationParser();
            return builder.AddInMemoryCollection(parser.Parse(input));
        }
    }
}
