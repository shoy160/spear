using Spear.Core;

namespace Spear.Nacos
{
    public class NacosConfig
    {
        public string Host { get; set; }
        public string Tenant { get; set; }
        public string Group { get; set; } = "DEFAULT_GROUP";
        public string Applications { get; set; }

        /// <summary> 轮询间隔(秒) </summary>
        public long Interval { get; set; } = 120;

        public long LongPollingTimeout { get; set; } = 30000;

        public static NacosConfig Config()
        {
            return "nacos".Config<NacosConfig>() ?? new NacosConfig();
        }
    }
}
