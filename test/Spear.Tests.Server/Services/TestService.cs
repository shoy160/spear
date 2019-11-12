using Microsoft.Extensions.Logging;
using Spear.Tests.Contracts;
using System;
using System.Threading.Tasks;
using Spear.Core.Session;

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
            _logger.LogInformation($"{_session.UserName} {DateTime.Now:u} -> notify name:{name}");
            await Task.CompletedTask;
        }

        public async Task<string> Get(string name)
        {
            return await Task.FromResult($"{_session.UserName}, get name:{name}");
        }
    }
}
