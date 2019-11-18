using System.Collections.Generic;

namespace Spear.Nacos.Sdk.Requests.Service
{
    public class InstanceBeat
    {
        public int port { get; set; }
        public string ip { get; set; }
        public double weight { get; set; }
        public string serviceName { get; set; }
        public string cluster { get; set; }
        public IDictionary<string, string> metadata { get; set; } = new Dictionary<string, string>();
        public bool scheduled { get; set; }
    }
}
