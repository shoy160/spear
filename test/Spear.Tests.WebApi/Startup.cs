using Microsoft.Extensions.DependencyInjection;
using Spear.Core.Data;
using Spear.Dapper.Mysql;
using Spear.WebApi;

namespace Spear.Tests.WebApi
{
    public class Startup : DStartup
    {
        public Startup() : base("基础网关接口", docType: DocumentType.SwaggerUi) { }

        public override void ConfigureServices(IServiceCollection services)
        {
            DbConnectionManager.AddAdapter(new MySqlConnectionAdapter());
            base.ConfigureServices(services);
        }
    }
}
