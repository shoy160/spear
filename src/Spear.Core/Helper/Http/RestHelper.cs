using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Spear.Core.Dependency;
using Spear.Core.Exceptions;
using Spear.Core.Extensions;
using Spear.Core.Timing;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Spear.Core.Helper.Http
{
    /// <summary> 接口调用辅助 </summary>
    public class RestHelper
    {
        private readonly string _baseUri;
        private const string Prefix = "sites:";
        private readonly ILogger _logger;
        private readonly HttpHelper _httpHelper;
        private readonly int _timeout;

        /// <summary> 构造函数 </summary>
        /// <param name="baseUri"></param>
        /// <param name="timeout">超时时间(秒)</param>
        public RestHelper(string baseUri = null, int timeout = -1)
        {
            _baseUri = baseUri;
            _httpHelper = HttpHelper.Instance;
            _timeout = timeout;
            _logger = CurrentIocManager.CreateLogger<RestHelper>();
        }

        /// <inheritdoc />
        /// <summary> 构造函数 </summary>
        /// <param name="siteEnum"></param>
        public RestHelper(Enum siteEnum) : this(
            $"{Prefix}{siteEnum.ToString().ToLower()}".Config<string>())
        {
        }

        private static string GetTicket()
        {
            var key = "ticket".Config<string>();
            var timestamp = Clock.Now.ToTimestamp();
            return $"{timestamp}{EncryptHelper.Hash($"{key}{timestamp}", EncryptHelper.HashFormat.MD532).ToLower()}";
        }

        public async Task<HttpResponseMessage> RequestAsync(HttpRequest request, HttpMethod method = null,
            bool ticket = false)
        {
            if (string.IsNullOrWhiteSpace(request.Url))
                throw new BusiException("Http请求参数异常");
            if (!string.IsNullOrWhiteSpace(_baseUri))
            {
                var uri = new Uri(new Uri(_baseUri), request.Url);
                request.Url = uri.AbsoluteUri;
            }

            request.Headers = request.Headers ?? new Dictionary<string, string>();
            if (ticket)
                request.Headers.Add("App-Ticket", GetTicket());
            if (_timeout > 0)
                request.Timeout = TimeSpan.FromSeconds(_timeout);
            return await _httpHelper.RequestAsync(method ?? HttpMethod.Get, request);
        }

        /// <summary> 请求接口 </summary>
        /// <param name="request"></param>
        /// <param name="method"></param>
        /// <param name="ticket"></param>
        /// <returns></returns>
        public async Task<string> RequestStringAsync(HttpRequest request, HttpMethod method = null, bool ticket = false)
        {
            var resp = await RequestAsync(request, method, ticket);
            if (resp.IsSuccessStatusCode)
                return await resp.ReadAsStringAsync(request.Encoding);
            if (_logger.IsEnabled(LogLevel.Warning))
                _logger.LogWarning($"http request fail:{resp.StatusCode}");
            return string.Empty;
        }

        /// <summary> 获取API接口返回的实体对象 </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="request"></param>
        /// <param name="method"></param>
        /// <param name="def"></param>
        /// <returns></returns>
        public async Task<T> RequestAsync<T>(HttpRequest request, HttpMethod method = null, T def = default)
        {
            try
            {
                var html = await RequestStringAsync(request, method);
                if (!string.IsNullOrWhiteSpace(html))
                {
                    var setting = new JsonSerializerSettings();
                    setting.Converters.Add(new DateTimeConverter());
                    return JsonConvert.DeserializeObject<T>(html, setting);
                }
            }
            catch (Exception ex)
            {
                if (ex is BusiException)
                {
                    throw;
                }

                _logger.LogError(ex, ex.Message);
            }

            return def;
        }

        /// <summary> GET </summary>
        /// <param name="api"></param>
        /// <param name="paras"></param>
        /// <param name="headers"></param>
        /// <returns></returns>
        public async Task<string> GetAsync(string api, object paras = null, IDictionary<string, string> headers = null)
            => await RequestStringAsync(new HttpRequest(api) { Params = paras, Headers = headers }, HttpMethod.Get);

        /// <summary> GET </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="api"></param>
        /// <param name="paras"></param>
        /// <param name="headers"></param>
        /// <param name="def"></param>
        /// <returns></returns>
        public async Task<T> GetAsync<T>(string api, object paras = null, IDictionary<string, string> headers = null,
            T def = default)
            => await RequestAsync(new HttpRequest(api) { Params = paras, Headers = headers }, HttpMethod.Get, def);

        /// <summary> POST </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<string> PostAsync(HttpRequest request) => await RequestStringAsync(request, HttpMethod.Post);

        /// <summary> POST </summary>
        /// <param name="url"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task<string> PostAsync(string url, object data) =>
            await RequestStringAsync(new HttpRequest(url) { Data = data }, HttpMethod.Post);

        /// <summary> POST </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<T> PostAsync<T>(HttpRequest request) => await RequestAsync<T>(request, HttpMethod.Post);

        /// <summary> POST </summary>
        /// <param name="url"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task<T> PostAsync<T>(string url, object data) =>
            await RequestAsync<T>(new HttpRequest(url) { Data = data }, HttpMethod.Post);

        /// <summary> PUT </summary>
        /// <param name="url"></param>
        /// <param name="param"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task<string> PutAsync(string url, object param = null, object data = null) =>
            await RequestStringAsync(new HttpRequest(url) { Params = param, Data = data }, HttpMethod.Put);

        /// <summary> PUT </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<string> PutAsync(HttpRequest request) => await RequestStringAsync(request, HttpMethod.Put);

        /// <summary> PUT </summary>
        /// <param name="url"></param>
        /// <param name="param"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task<T> PutAsync<T>(string url, object param = null, object data = null) =>
            await RequestAsync<T>(new HttpRequest(url) { Params = param, Data = data }, HttpMethod.Put);

        /// <summary> PUT </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<T> PutAsync<T>(HttpRequest request) => await RequestAsync<T>(request, HttpMethod.Put);

        /// <summary> DELETE </summary>
        /// <param name="api"></param>
        /// <param name="paras"></param>
        /// <param name="headers"></param>
        /// <returns></returns>
        public async Task<string> DeleteAsync(string api, object paras = null,
            IDictionary<string, string> headers = null)
            => await RequestStringAsync(new HttpRequest(api) { Params = paras, Headers = headers }, HttpMethod.Delete);

        /// <summary> DELETE </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="api"></param>
        /// <param name="paras"></param>
        /// <param name="headers"></param>
        /// <returns></returns>
        public async Task<T> DeleteAsync<T>(string api, object paras = null, IDictionary<string, string> headers = null)
            => await RequestAsync<T>(new HttpRequest(api)
            {
                Params = paras,
                Headers = headers
            }, HttpMethod.Delete);
    }
}
