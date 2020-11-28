using System.Collections.Generic;
using Spear.Core.Extensions;
using Spear.Core.Micro.Services;

namespace Spear.Core.Config
{
    public class SpearConfig
    {
        private const string ConfigPrefix = "spear";

        /// <summary> 服务地址 </summary>
        public ServiceAddress Service { get; set; } = new ServiceAddress();

        /// <summary> 服务列表 </summary>
        public IDictionary<string, ServiceAddress[]> Services { get; set; } =
            new Dictionary<string, ServiceAddress[]>();

        /// <summary> 获取配置 </summary>
        /// <returns></returns>
        public static SpearConfig GetConfig()
        {
            return ConfigPrefix.Config(new SpearConfig());
        }
    }
}
