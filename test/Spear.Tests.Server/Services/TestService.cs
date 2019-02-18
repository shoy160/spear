using Microsoft.Extensions.Logging;
using Spear.Tests.Contracts;
using System;
using System.Threading.Tasks;

namespace Spear.Tests.Server.Services
{
    public class TestService : ITestContract
    {
        private readonly ILogger<TestService> _logger;

        public TestService(ILogger<TestService> logger)
        {
            _logger = logger;
        }

        public async Task Notice(string name)
        {
            _logger.LogInformation($"{DateTime.Now:u} -> get name:{name}");
            await Task.CompletedTask;
        }

        public async Task<string> Get(string name)
        {
            return await Task.FromResult($"your name:{name}");
        }
    }
}
