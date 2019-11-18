using System.Collections.Generic;

namespace Spear.Nacos.Sdk.Results
{
    public class ListInstanceResult
    {
        public string Dom { get; set; }
        public int CacheMillis { get; set; }
        public string UseSpecifiedURL { get; set; }
        public List<InstanceHost> Hosts { get; set; }
        public string Checksum { get; set; }
        public long LastRefTime { get; set; }
        public string Env { get; set; }
        public string Clusters { get; set; }
    }
}
