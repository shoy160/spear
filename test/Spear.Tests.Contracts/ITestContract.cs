using Acb.Core;
using Acb.Core.Dependency;
using System.Threading.Tasks;

namespace Spear.Tests.Contracts
{
    public interface ITestContract : IMicroService, IDependency
    {
        Task Notice(string name);
        Task<string> Get(string name);
    }
}
