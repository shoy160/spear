using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Spear.Sdk.Core.Dtos;
using Spear.Sdk.Core.Helper;
#if NETSTANDARD2_0
using Microsoft.AspNetCore.Http;
#else
using System.Web;
#endif

namespace Spear.Sdk.Core
{
    public abstract class SdkService
    {
        /// <summary> 配置信息 </summary>
        protected readonly ISdkConfig Config;

        /// <summary> 请求辅助 </summary>
        protected readonly HttpHelper HttpHelper;

        /// <summary> 异常事件 </summary>
        public event RequestResultHandler OnError;

#if NETSTANDARD2_0
        protected readonly IHttpContextAccessor ContextAccessor;

        protected SdkService(ISdkConfig config, IHttpContextAccessor contextAccessor)
        {
            Config = config;
            HttpHelper = new HttpHelper(config.Gateway);
            HttpHelper.OnRequest += BeforeRequest;
            HttpHelper.OnResult += OnResult;
            ContextAccessor = contextAccessor;
        }
#else
        protected SdkService(ISdkConfig config)
        {
            Config = config;
            HttpHelper = new HttpHelper(config.Gateway);
            HttpHelper.OnRequest += BeforeRequest;
            HttpHelper.OnResult += OnResult;
        }
#endif


        protected virtual void BeforeRequest(WebRequest req)
        {

        }

        /// <summary> 当前请求上下文 </summary>
        protected HttpContext Current
        {
            get
            {
#if NETSTANDARD2_0
                return ContextAccessor?.HttpContext;
#else
                return HttpContext.Current;
#endif
            }
        }

        /// <summary> 当前请求Body </summary>
        protected Stream Body
        {
            get
            {
#if NETSTANDARD2_0
                return Current.Request.Body;

#else
                return Current.Request.InputStream;
#endif
            }
        }

        /// <summary> Json反序列化 </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="json"></param>
        /// <param name="def"></param>
        /// <returns></returns>
        protected T Json<T>(string json, T def = default)
        {
            return JsonHelper.Json(json, def);
        }

        /// <summary> Json序列化 </summary>
        /// <param name="obj"></param>
        /// <param name="indented"></param>
        /// <returns></returns>
        protected string ToJson(object obj, bool indented = false)
        {
            return JsonHelper.ToJson(obj, indented);
        }

        /// <summary> 模型验证 </summary>
        /// <param name="obj"></param>
        /// <param name="items"></param>
        /// <param name="required"></param>
        protected void Validate(object obj, Dictionary<object, object> items = null, bool required = true)
        {
            if (obj == null)
            {
                if (required)
                    throw new ArgumentNullException(nameof(obj), "参数不能为空");
                return;
            }
            var validationContext = new ValidationContext(obj, items);
            var results = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(obj, validationContext, results, true);

            if (isValid) return;
            var error = results.First();
            throw new ArgumentNullException(error.MemberNames.FirstOrDefault(), error.ErrorMessage);
        }

        /// <summary> 请求是否成功 </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        protected virtual bool IsSuccess(SdkRequestData data)
        {
            if (data == null || data.Exception != null || data.Code != (int)HttpStatusCode.OK)
                return false;
            var t = Json<SdkResult>(data.Result);
            return t?.Status ?? false;
        }

        /// <summary> 请求结果 </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        protected virtual async Task OnResult(SdkRequestData result)
        {
            if (OnError == null || !IsSuccess(result))
                return;
            if (OnError != null)
                await OnError(result);
        }
    }
}
