using Acb.Core;
using Acb.Core.Dependency;
using Spear.Core;
using System.Threading.Tasks;

namespace Spear.Tests.Contracts
{
    [ServiceRoute("test")]
    public interface ITestContract : IMicroService, IDependency
    {
        Task Notice(string name);
        Task<string> Get(string name);
    }
}
