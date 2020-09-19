using Spear.Core.Serialize;

namespace Spear.Tests.Client.Services.Impl
{
    [Naming("serviceA")]
    public class ServieA : IService
    {
        public string Name(string name)
        {
            return $"service A : {name}";
        }
    }
}
