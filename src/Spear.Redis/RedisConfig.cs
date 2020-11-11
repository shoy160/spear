using Spear.Core.Extensions;

namespace Spear.Redis
{
    /// <summary> Redis连接配置 </summary>
    public class RedisConfig
    {
        private const string Region = "redis";
        private const string DefaultConfigName = "redisDefault";
        private const string DefaultName = "default";

        public static RedisConfig Config(string configName = null)
        {
            if (string.IsNullOrWhiteSpace(configName))
                configName = DefaultConfigName.Config(DefaultName);

            return new RedisConfig
            {
                Name = configName,
                ConnectionString = $"{Region}:{configName}".Config<string>()
            };
        }

        public string Name { get; set; }
        public string ConnectionString { get; set; }
    }
}
