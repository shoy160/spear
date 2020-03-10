using System;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Spear.Core;
using Spear.Core.Session;
using Spear.Tests.Contracts;

namespace Spear.Tests.Server.Services
{
    public class AccountService : Account.AccountBase//, ISpearService
    {
        private readonly ILogger<AccountService> _logger;
        private readonly IMicroSession _session;

        public AccountService(ILogger<AccountService> logger, IMicroSession session)
        {
            _logger = logger;
            _session = session;
        }

        public override Task<LoginReply> Login(LoginRequest request, ServerCallContext context)
        {
            _logger.LogInformation(JsonConvert.SerializeObject(request));
            _logger.LogInformation(JsonConvert.SerializeObject(_session));
            var model = new LoginReply
            {
                Id = Guid.NewGuid().ToString("N"),
                Token = Guid.NewGuid().ToString("N").Substring(8, 16)
            };
            return Task.FromResult(model);
        }
    }
}
