using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;

namespace Spear.Core.Micro.Services
{
    public enum ServiceProtocol
    {
        Tcp,
        Http,
        Ws
    }

    public class ServiceAddress
    {
        public IPAddress Ip { get; set; }
        public ServiceProtocol Protocol { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        /// <summary> 对外注册的服务地址(ip或DNS) </summary>
        public string Service { get; set; }

        /// <summary> 权重 </summary>
        public double Weight { get; set; } = 1;

        public ServiceAddress() { }

        public ServiceAddress(string host, int port)
        {
            Host = host;
            Port = port;
        }

        public string IpAddress => string.IsNullOrWhiteSpace(Service) ? Host : Service;

        public override string ToString()
        {
            if (string.IsNullOrWhiteSpace(Host))
                Host = Ip.ToString();
            return $"{this.Address()}:{Port}";
        }
    }

    public static class ServiceAddressExtensions
    {
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
            if (string.IsNullOrWhiteSpace(service) || service == "localhost")
            {
                return new IPEndPoint(IPAddress.Any, address.Port);
            }

            if (service.IsIp())
                return new IPEndPoint(IPAddress.Parse(service), address.Port);
            return new DnsEndPoint(service, address.Port);
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
            //权重随机
            if (services == null || !services.Any()) return null;
            if (services.Count() == 1) return services.First();
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
