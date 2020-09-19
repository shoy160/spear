using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;

using Spear.Core.Dependency;

namespace Spear.WebApi.Activators
{
    /// <summary> 属性控制器 </summary>
    public class PropertyControllerActivator : IControllerActivator
    {
        /// <summary> 创建Controller </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public object Create(ControllerContext context)
        {
            var serviceType = context.ActionDescriptor.ControllerTypeInfo.AsType();
            return context.HttpContext.RequestServices.PropResolve(serviceType);
        }

        public virtual void Release(ControllerContext context, object controller)
        {
        }
    }
}
