using System;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Spear.WebApi.Validations
{
    public class SpearEnumTypeModelBinderProvider : IModelBinderProvider
    {
        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (context.Metadata.IsEnum)
            {
                var loggerFactory = context.Services.GetRequiredService<ILoggerFactory>();
                return new SpearEnumTypeModelBinder(
                    true,
                    context.Metadata.UnderlyingOrModelType,
                    loggerFactory);
            }

            return null;
        }
    }
}
