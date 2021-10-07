using Spear.Core.Config;
using Spear.Core.Extensions;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace Spear.Core
{
    public static class Constants
    {

        /// <summary> 产品模式 </summary>
        public static ProductMode Mode
        {
            get
            {
                var mode = "SPEAR_MODE".Env<ProductMode?>();
                if (mode.HasValue)
                    return mode.Value;
                return "mode".Config(ProductMode.Dev);
            }
        }

        /// <summary> 是否开发环境 </summary>
        public static bool IsDev => Mode == ProductMode.Dev;

        /// <summary> 是否测试环境 </summary>
        public static bool IsTest => Mode == ProductMode.Test;

        /// <summary> 是否预发布环境 </summary>
        public static bool IsReady => Mode == ProductMode.Ready;

        /// <summary> 是否正式环境 </summary>
        public static bool IsProd => Mode == ProductMode.Prod;

        private static string _localIp;

        /// <summary> 获取本地IP </summary>
        /// <returns></returns>
        public static string LocalIp()
        {
            if (!string.IsNullOrWhiteSpace(_localIp))
                return _localIp;
            return _localIp = NetworkInterface.GetAllNetworkInterfaces().Select(p => p.GetIPProperties())
                .SelectMany(p => p.UnicastAddresses).FirstOrDefault(p =>
                    p.Address.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(p.Address))?.Address?.ToString();
        }

        public static ServiceProtocol Protocol { get; set; }

        public static ServiceCodec Codec { get; set; }

    }
}
