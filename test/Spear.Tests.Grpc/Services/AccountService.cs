using System.Threading.Tasks;
using Grpc.Core;

namespace Spear.Tests.Grpc.Services
{
    public class AccountService : Account.AccountBase
    {
        public override async Task<LoginReply> Login(LoginRequest request, ServerCallContext context)
        {
            var model = new LoginReply
            {
                Id = "sdf",
                Token = ""
            };
            return await Task.FromResult(model);
        }
    }
}
