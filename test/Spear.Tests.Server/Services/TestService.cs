using Spear.Tests.Contracts;
using System;
using System.Threading.Tasks;

namespace Spear.Tests.Server.Services
{
    public class TestService : ITestContract
    {
        public async Task Notice(string name)
        {
            Console.WriteLine($"get name:{name}");
            await Task.CompletedTask;
        }

        public async Task<string> Get(string name)
        {
            return await Task.FromResult($"your name:{name}");
        }
    }
}
