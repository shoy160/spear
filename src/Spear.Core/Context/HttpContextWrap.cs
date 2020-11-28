using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Spear.Core.Dependency;
using Spear.Core.Extensions;
using Spear.Core.Helper;
using Spear.Core.Session;

namespace Spear.Core.Context
{
    /// <summary> 请求上下文包装 </summary>
    public class HttpContextWrap
    {
        private const string DefaultIp = "127.0.0.1";

        /// <summary> 当前上下文 </summary>
        public readonly HttpContext Current;

        /// <summary> 当前上下文 </summary>
        /// <param name="current"></param>
        public HttpContextWrap(HttpContext current)
        {
            current?.Request?.EnableBuffering();
            Current = current;
        }

        public string GetHeader(string key, string def = null)
        {
            if (Current != null && Current.Request.Headers.TryGetValue(key, out var value))
                return value;
            return def;
        }

        private string _remoteIp;

        /// <summary> 客户端IP </summary>
        public string RemoteIpAddress
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(_remoteIp))
                    return _remoteIp;
                var context = Current;
                if (context == null)
                    return _remoteIp = DefaultIp;

                string GetIpFromHeader(string key)
                {

                    if (!context.Request.Headers.TryGetValue(key, out var ips))
                        return string.Empty;
                    foreach (var ip in ips)
                    {
                        if (RegexHelper.IsIp(ip)) return ip;
                    }

                    return string.Empty;
                }
                //获取代理IP
                var userHostAddress = GetIpFromHeader(SpearClaimTypes.HeaderForward);
                if (!string.IsNullOrWhiteSpace(userHostAddress))
                    return _remoteIp = userHostAddress;
                userHostAddress = GetIpFromHeader(SpearClaimTypes.HeaderRealIp);
                if (!string.IsNullOrWhiteSpace(userHostAddress))
                    return _remoteIp = userHostAddress;
                userHostAddress = GetIpFromHeader("HTTP_X_FORWARDED_FOR");
                if (!string.IsNullOrWhiteSpace(userHostAddress))
                    return _remoteIp = userHostAddress;
                userHostAddress = GetIpFromHeader("REMOTE_ADDR");
                if (!string.IsNullOrWhiteSpace(userHostAddress))
                    return _remoteIp = userHostAddress;
                userHostAddress = context.Connection.RemoteIpAddress.ToString();
                return _remoteIp = RegexHelper.IsIp(userHostAddress) ? userHostAddress : DefaultIp;
            }
        }

        private string _localIp;

        /// <summary> 本地IP </summary>
        public string LocalIpAddress
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(_localIp))
                    return _localIp;
                if (Current == null)
                    return _localIp = Constants.LocalIp() ?? DefaultIp;
                var ip = Current.Connection.LocalIpAddress.ToString();
                if (string.IsNullOrEmpty(ip) || !RegexHelper.IsIp(ip))
                {
                    return _localIp = Constants.LocalIp();
                }

                return _localIp = ip;
            }
        }

        /// <summary> 请求类型 </summary>
        public string RequestType => Current?.Request.Method;

        /// <summary> 表单 </summary>
        public IFormCollection Form => Current?.Request.Form;

        /// <summary> 请求体 </summary>
        public Stream Body => Current?.Request.Body;

        /// <summary> 用户代理 </summary>
        public string UserAgent => GetHeader(SpearClaimTypes.HeaderUserAgent, string.Empty);

        public string Authorization => GetHeader(SpearClaimTypes.HeaderAuthorization);

        public bool NewTrace { get; private set; }

        private string _traceId;

        public string TraceId
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(_traceId))
                    return _traceId;
                var traceId = GetHeader(SpearClaimTypes.HeaderTraceId);
                if (!string.IsNullOrWhiteSpace(traceId))
                    return _traceId = traceId;
                var accessor = CurrentIocManager.Resolve<IPrincipalAccessor>();
                if (accessor != null)
                    traceId = accessor.GetValue(SpearClaimTypes.TraceId);
                if (!string.IsNullOrWhiteSpace(traceId))
                    return _traceId = traceId;
                NewTrace = true;
                traceId = IdentityHelper.Guid32;
                accessor.SetValue(SpearClaimTypes.TraceId, traceId);
                return _traceId = traceId;
            }
        }

        /// <summary> 来源 </summary>
        public string Referer => GetHeader(SpearClaimTypes.HeaderReferer);

        /// <summary> 内容类型 </summary>
        public string ContentType => Current?.Request.ContentType;

        /// <summary> 参数 </summary>
        public string QueryString => Current?.Request.QueryString.ToUriComponent();

        /// <summary> 获取原始Url </summary>
        /// <returns></returns>
        public static string GetRawUrl(HttpRequest request)
        {
            if (request == null)
                return string.Empty;
            try
            {
                return $"{request.Scheme}://{request.Host}{request.Path.Value}{request.QueryString.Value}";
            }
            catch
            {
                return string.Empty;
            }
        }
        /// <summary> 原始地址 </summary>
        public string RawUrl => GetRawUrl(Current?.Request);

        /// <summary> 获取客户端IP </summary>
        public string ClientIp => RemoteIpAddress;

        /// <summary> 字典化对象 </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static IDictionary<string, object> FromForm(IFormCollection source)
        {
            if (source == null || !source.Keys.Any())
                return new Dictionary<string, object>();
            return source.Keys.ToDictionary(k => k.ToString(), v =>
            {
                if (source.TryGetValue(v, out var value))
                    return (object)value.ToString();
                return null;
            });
        }
        /// <summary> 请求体 </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public async Task<T> FromBody<T>()
        {
            if (ContentType == "application/x-www-form-urlencoded")
                return FromForm(Form).ToObject<T>();
            return await Body.ReadAsync<T>(ContentType);
        }
    }
}
