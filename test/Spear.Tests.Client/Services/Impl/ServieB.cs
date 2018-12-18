using Acb.Core.Serialize;

namespace Spear.Tests.Client.Services.Impl
{
    [Naming("serviceB")]
    public class ServieB : IService
    {
        public string Name(string name)
        {
            return $"service B : {name}";
        }
    }
}
