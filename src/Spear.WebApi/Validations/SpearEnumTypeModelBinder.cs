using System;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.Extensions.Logging;

namespace Spear.WebApi.Validations
{
    public class SpearEnumTypeModelBinder : EnumTypeModelBinder
    {
        public SpearEnumTypeModelBinder(bool suppressBindingUndefinedValueToEnumType, Type modelType,
            ILoggerFactory loggerFactory)
            : base(suppressBindingUndefinedValueToEnumType, modelType, loggerFactory)
        {
        }

        protected override void CheckModel(ModelBindingContext bindingContext, ValueProviderResult valueProviderResult, object model)
        {
            if (model == null)
            {
                base.CheckModel(bindingContext, valueProviderResult, null);
            }
            else
            {
                bindingContext.Result = ModelBindingResult.Success(model);
            }
        }
    }
}
