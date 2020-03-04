using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Spear.Core.Session;
using Spear.Tests.Contracts;
using Spear.Tests.Contracts.Dtos;
using Spear.Tests.Contracts.Enums;

namespace Spear.Tests.Server.Services
{
    public class TestService : ITestContract
    {
        private readonly ILogger<TestService> _logger;
        private readonly IMicroSession _session;

        public TestService(ILogger<TestService> logger, IMicroSession session)
        {
            _logger = logger;
            _session = session;
        }

        public async Task Notice(string name)
        {
            _logger.LogInformation($"{DateTime.Now:u} ->{_session.UserName} notify name:{name}");
            await Task.CompletedTask;
        }

        public async Task<string> Get(string name)
        {
            return await Task.FromResult($"{_session.UserName}, get name:{name}");
        }

        public async Task<UserDto> User(UserInputDto input)
        {
            _logger.LogInformation($"{DateTime.Now:u} ->{_session.UserName} {JsonConvert.SerializeObject(input)}");
            var user = new UserDto
            {
                Id = input.Id,
                Name = input.Name,
                Role = UserRole.Admin,
                Birthday = DateTime.Now
            };
            return await Task.FromResult(user);
        }
    }
}
