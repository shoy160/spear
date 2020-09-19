using Spear.Core.Extensions;

namespace Spear.Nacos
{
    public class NacosConfig
    {
        public string Host { get; set; }
        public string Tenant { get; set; }
        public string Group { get; set; } = "DEFAULT_GROUP";
        public string Applications { get; set; }

        /// <summary> 轮询间隔(秒,默认：120) </summary>
        public int Interval { get; set; } = 120;

        /// <summary>
        /// 长轮询超时时间(毫秒,默认:30000)
        /// </summary>
        public int LongPollingTimeout { get; set; } = 30000;

        public static NacosConfig Config()
        {
            return "nacos".Config<NacosConfig>() ?? new NacosConfig();
        }
    }
}
