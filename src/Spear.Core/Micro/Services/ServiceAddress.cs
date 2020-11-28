using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using Newtonsoft.Json;
using Spear.Core.Config;
using Spear.Core.Extensions;

namespace Spear.Core.Micro.Services
{
    public class ServiceAddress
    {
        /// <summary> IP </summary>
        public IPAddress Ip { get; set; }
        /// <summary> 服务协议 </summary>
        public ServiceProtocol Protocol { get; set; }
        /// <summary> Host </summary>
        public string Host { get; set; }

        private int _port;

        /// <summary> 端口号 </summary>
        public int Port
        {
            get => _port > 80 ? _port : 5000;
            set => _port = value;
        }

        private string _service;

        /// <summary> 对外注册的服务地址(ip或DNS) </summary>
        public string Service
        {
            get => string.IsNullOrWhiteSpace(_service) ? Host : _service;
            set => _service = value;
        }

        /// <summary> 对外服务端口 </summary>
        public int ServicePort { get; set; }

        /// <summary> 权重 </summary>
        public double Weight { get; set; } = 1;

        /// <summary> 是否开启Gzip </summary>
        public bool Gzip { get; set; } = true;

        /// <summary> 服务编码 </summary>
        public ServiceCodec Codec { get; set; }

        public ServiceAddress() { }

        public ServiceAddress(string host, int port)
        {
            Host = host;
            Port = port;
            ServicePort = port;
        }

        public string IpAddress => string.IsNullOrWhiteSpace(Service) ? Host : Service;

        public override string ToString()
        {
            if (string.IsNullOrWhiteSpace(Host) && string.IsNullOrWhiteSpace(Service) && Ip != null)
                Host = Ip.ToString();
            var port = ServicePort <= 0 ? Port : ServicePort;
            return $"{this.Address()}:{port}";
        }
    }

    /// <summary> 服务地址扩展 </summary>
    public static class ServiceAddressExtensions
    {
        /// <summary> 服务地址 </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public static string Address(this ServiceAddress address)
        {
            return $"{address.Protocol.ToString().ToLower()}://{address.IpAddress}";
        }

        public static string ToJson(this ServiceAddress address)
        {
            return JsonConvert.SerializeObject(address);
        }

        public static EndPoint ToEndPoint(this ServiceAddress address, bool isServer = true)
        {
            var service = isServer ? address.Host : address.Service;
            var port = isServer ? address.Port : (address.ServicePort <= 0 ? address.Port : address.ServicePort);

            if (string.IsNullOrWhiteSpace(service) || service == "localhost" || service == "*")
            {
                return new IPEndPoint(IPAddress.Any, port);
            }

            if (service.IsIp())
                return new IPEndPoint(IPAddress.Parse(service), port);
            return new DnsEndPoint(service, port);
        }

        /// <summary>
        /// 获取线程级随机数
        /// </summary>
        /// <returns></returns>
        private static Random Random()
        {
            var bytes = new byte[4];
            var rng =
                new RNGCryptoServiceProvider();
            rng.GetBytes(bytes);
            var seed = BitConverter.ToInt32(bytes, 0);
            var tick = DateTime.Now.Ticks + (seed);
            return new Random((int)(tick & 0xffffffffL) | (int)(tick >> 32));
        }

        /// <summary> 权重随机 </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static ServiceAddress Random(this IList<ServiceAddress> services)
        {
            if (services == null || !services.Any()) return null;
            if (services.Count == 1) return services.First();

            //权重随机
            var sum = services.Sum(t => t.Weight);
            var rand = Random().NextDouble() * sum;
            var tempWeight = 0D;
            foreach (var service in services)
            {
                tempWeight += service.Weight;
                if (rand <= tempWeight)
                    return service;
            }

            return services.RandomSort().First();
        }
    }
}
