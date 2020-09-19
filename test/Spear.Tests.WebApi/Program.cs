using Spear.WebApi;
using System.Threading.Tasks;

namespace Spear.Tests.WebApi
{
    public class Program : DHost<Startup>
    {
        public static async Task Main(string[] args)
        {
            await Start(args);
        }
    }
}
