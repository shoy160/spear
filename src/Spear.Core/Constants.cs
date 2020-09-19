using Spear.Core.Config;
using Spear.Core.Extensions;
using System;
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
                var mode = Environment.GetEnvironmentVariable("SPEAR_MODE");
                return mode.CastTo(ProductMode.Dev);
            }
        }

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
