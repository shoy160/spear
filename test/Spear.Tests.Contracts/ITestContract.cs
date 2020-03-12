using System.Threading.Tasks;
using Spear.Core;
using Spear.Tests.Contracts.Dtos;

namespace Spear.Tests.Contracts
{
    [ServiceRoute("test")]
    public interface ITestContract //: ISpearService
    {
        Task Notice(string name);

        Task<string> Get(string name);

        Task<UserDto> User(UserInputDto input);
    }
}
