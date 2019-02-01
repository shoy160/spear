using Newtonsoft.Json;
using System.Net;

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
        public ServiceProtocol Protocol { get; } = Constants.Protocol;
        public string Host { get; set; }
        public int Port { get; set; }
        /// <summary> 对外注册的服务地址(ip或DNS) </summary>
        public string Service { get; set; }

        public ServiceAddress() { }

        public ServiceAddress(string host, int port)
        {
            Host = host;
            Port = port;
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }

        public string Address()
        {
            return $"{Protocol.ToString().ToLower()}://{Host}";
        }

        public override string ToString()
        {
            if (string.IsNullOrWhiteSpace(Host))
                Host = Ip.ToString();
            return $"{Address()}:{Port}";
        }

        public EndPoint ToEndPoint(bool isHost = true)
        {
            var service = isHost ? Host : Service;
            if (string.IsNullOrWhiteSpace(service) || service == "localhost")
            {
                return new IPEndPoint(IPAddress.Loopback, Port);
            }

            if (service.IsIp())
                return new IPEndPoint(IPAddress.Parse(service), Port);
            return new DnsEndPoint(service, Port);
        }
    }
}
