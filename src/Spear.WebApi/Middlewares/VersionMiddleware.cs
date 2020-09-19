using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Spear.WebApi.Middlewares
{
    public class VersionMiddleware
    {
        private readonly RequestDelegate _next;

        public VersionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            if (!string.IsNullOrWhiteSpace(RuntimeInformation.FrameworkDescription))
            {
                context.Response.Headers.TryAdd("core-version",
                    RuntimeInformation.FrameworkDescription.Replace(".NET Core ", string.Empty));
            }
            context.Response.Headers.TryAdd("fw-version", "1.0.1");
            await _next(context);
        }
    }
}
