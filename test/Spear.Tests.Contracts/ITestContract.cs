using Spear.Core;
using System.Threading.Tasks;

namespace Spear.Tests.Contracts
{
    [ServiceRoute("test")]
    public interface ITestContract : ISpearService
    {
        Task Notice(string name);
        Task<string> Get(string name);
    }
}
