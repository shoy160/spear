using System.Collections.Generic;

namespace Spear.Nacos.Sdk.Results
{
    public class InstanceHost
    {
        public bool Valid { get; set; }
        public bool Marked { get; set; }
        public string InstanceId { get; set; }
        public int Port { get; set; }
        public string Ip { get; set; }
        public double Weight { get; set; }
        public IDictionary<string, string> Metadata { get; set; }
    }
}
