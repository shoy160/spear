using Acb.Core.Helper;
using Acb.Core.Serialize;
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
        public ServiceProtocol Protocol { get; set; } = ServiceProtocol.Tcp;
        public string Host { get; set; }
        public int Port { get; set; }

        public ServiceAddress() { }

        public ServiceAddress(string host, int port)
        {
            Host = host;
            Port = port;
        }

        public string ToJson()
        {
            return JsonHelper.ToJson(this);
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

        public EndPoint ToEndPoint()
        {
            if (RegexHelper.IsIp(Host))
                return new IPEndPoint(IPAddress.Parse(Host), Port);
            return new DnsEndPoint(Host, Port);
        }
    }
}
