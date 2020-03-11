using System;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using Spear.Core.Config;

namespace Spear.Core
{
    public static class Constants
    {
        /// <summary> MD5加密 </summary>
        /// <param name="data">数据</param>
        /// <param name="encoding">编码</param>
        /// <returns></returns>
        public static string Md5(this string data, Encoding encoding = null)
        {
            var md5 = System.Security.Cryptography.MD5.Create();
            encoding = encoding ?? Encoding.UTF8;
            var dataByte = md5.ComputeHash(encoding.GetBytes(data));
            var sb = new StringBuilder();
            foreach (var b in dataByte)
            {
                sb.Append(b.ToString("X2"));
            }

            return sb.ToString();
        }

        /// <summary> 程序集key </summary>
        /// <param name="assembly"></param>
        /// <returns></returns>
        public static string AssemblyKey(this Assembly assembly)
        {
            var assName = assembly.GetName();
            return $"{assName.Name}_{assName.Version}";
        }

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
