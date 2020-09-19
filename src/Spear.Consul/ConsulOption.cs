using Spear.Core.Extensions;

namespace Spear.Consul
{
    public class ConsulOption
    {
        public string Server { get; set; }
        public string Token { get; set; }

        public static ConsulOption Config()
        {
            return "consul".Config<ConsulOption>();
        }
    }
}
