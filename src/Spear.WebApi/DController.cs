using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Spear.Core;
using Spear.Core.Dependency;
using Spear.WebApi.Filters;
using System.Collections.Generic;

namespace Spear.WebApi
{
    /// <summary> 控制器基类 </summary>
    [ValidateModel(Order = 0)]
    public abstract class DController : ControllerBase
    {
        /// <summary> 当前请求上下文 </summary>
        protected HttpContext Current => ControllerContext.HttpContext;

        ///// <summary> 从body中读取对象 </summary>
        ///// <typeparam name="T"></typeparam>
        ///// <returns></returns>
        //[NonAction]
        //protected async Task<T> FromBody<T>()
        //{
        //    return await AcbHttpContext.FromBody<T>();
        //}

        /// <summary> 获取IOC注入 </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        protected T Resolve<T>()
        {
            return Current?.RequestServices != null
                ? Current.RequestServices.GetService<T>()
                : CurrentIocManager.Resolve<T>();
        }

        /// <summary> 身份验证 </summary>
        /// <param name="context"></param>
        [NonAction]
        public virtual void AuthorizeValidate(HttpContext context) { }

        #region Results

        /// <summary> 成功 </summary>
        protected DResult Success => DResult.Success;

        /// <summary> 失败 </summary>
        /// <param name="message"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        protected DResult Error(string message, int code = -1)
        {
            return DResult.Error(message, code);
        }

        /// <summary> 成功 </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        protected DResult<T> Succ<T>(T data)
        {
            return DResult.Succ(data);
        }

        /// <summary> 失败 </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="message"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        protected DResult<T> Error<T>(string message, int code = -1)
        {
            return DResult.Error<T>(message, code);
        }

        /// <summary> 成功 </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        protected DResults<T> Succ<T>(IEnumerable<T> list, int count = -1)
        {
            return DResult.Succ(list, count);
        }

        /// <summary> 失败 </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="message"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        protected DResults<T> Errors<T>(string message, int code = -1)
        {
            return DResult.Errors<T>(message, code);
        }

        #endregion
    }
}
