using Microsoft.Extensions.Logging;
using Spear.Core.Dependency;
using Spear.Core.Exceptions;
using Spear.Core.Extensions;
using Spear.Core.Serialize;
using Spear.Core.Session;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Spear.Core.Helper.Http
{
    /// <summary> Http请求类 </summary>
    public class HttpHelper
    {
        private readonly ILogger _logger;

        private static readonly IDictionary<string, string> DefaultHeaders = new Dictionary<string, string>
        {
            {"Accept-Encoding", "gzip, deflate"},
            {"Accept-Language", "zh-CN,zh;q=0.9"},
            {"Cache-Control", "max-age=0"}
        };

        private HttpHelper()
        {
            _logger = CurrentIocManager.CreateLogger<HttpHelper>();
        }

        private static (HttpClient, bool) CreateHttpClient(HttpRequest request)
        {
            HttpClient client;
            var isFactory = false;
            //if (CurrentIocManager.IsRegisted<IHttpClientFactory>())
            //{
            //    isFactory = true;
            //    var factory = CurrentIocManager.Resolve<IHttpClientFactory>();
            //    client = factory.CreateClient();
            //}
            //else
            //{
            client = new HttpClient(new HttpClientHandler { UseCookies = true });
            //}

            foreach (var header in DefaultHeaders)
            {
                client.DefaultRequestHeaders.Add(header.Key, header.Value);
            }


            if (request.Timeout.HasValue)
                client.Timeout = request.Timeout.Value;
            if (request.MaxBufferSize > 0)
                client.MaxResponseContentBufferSize = request.MaxBufferSize;
            return (client, isFactory);
        }

        /// <summary> 单例模式 </summary>
        public static HttpHelper Instance => Singleton<HttpHelper>.Instance ??
                                               (Singleton<HttpHelper>.Instance = new HttpHelper());


        /// <summary> 请求 </summary>
        /// <param name="method"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<HttpResponseMessage> RequestAsync(HttpMethod method, HttpRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.Url))
                throw new SpearException("Http请求参数异常");
            try
            {
                request.Encoding = request.Encoding ?? Encoding.UTF8;
                request.Headers = request.Headers ?? new Dictionary<string, string>();

                var url = request.Url;
                if (request.Params != null)
                {
                    url += url.IndexOf('?') > 0 ? "&" : "?";
                    url += request.Params.ToDictionary().ToUrl(encoding: request.Encoding);
                }

                var uri = new Uri(url);
                var req = new HttpRequestMessage(method, uri);
                if (request.Headers != null)
                {
                    foreach (var key in request.Headers)
                    {
                        if (string.IsNullOrWhiteSpace(key.Value))
                            continue;
                        req.Headers.TryAddWithoutValidation(key.Key, key.Value);
                    }
                }

                HttpContent content = null;
                if (request.Content != null)
                {
                    content = request.Content;
                }
                else if (request.Data != null && method != HttpMethod.Get)
                {
                    switch (request.BodyType)
                    {
                        case HttpBodyType.Json:
                            var json = request.Data is string
                                ? request.Data.ToString()
                                : JsonHelper.ToJson(request.Data);
                            content = new StringContent(json, request.Encoding, "application/json");
                            break;
                        case HttpBodyType.Form:
                            var str = string.Empty;
                            if (request.Data != null)
                            {
                                var type = request.Data.GetType();
                                if (type.IsSimpleType())
                                {
                                    str = request.Data.ToString();
                                }
                                else
                                {
                                    var dict = request.Data.ToDictionary();
                                    str = dict.ToUrl(true, request.Encoding);
                                }
                            }

                            content = new ByteArrayContent(request.Encoding.GetBytes(str));
                            content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
                            break;
                        case HttpBodyType.Xml:
                            var xml = request.Data is string
                                ? request.Data.ToString()
                                : request.Data.ToDictionary().ToXml();
                            content = new StringContent(xml, request.Encoding, "text/xml");
                            break;
                        case HttpBodyType.File:
                            if (request.Files != null && request.Files.Any())
                            {
                                var multiContent = new MultipartFormDataContent(DateTime.Now.Ticks.ToString("x"));
                                foreach (var file in request.Files)
                                {
                                    var stream = new MemoryStream();
                                    var buffer = new byte[checked((uint)Math.Min(4096, (int)file.Value.Length))];
                                    int bytesRead;
                                    while ((bytesRead = file.Value.Read(buffer, 0, buffer.Length)) != 0)
                                        stream.Write(buffer, 0, bytesRead);
                                    multiContent.Add(new StreamContent(stream), file.Key, file.Value.Name);
                                }

                                content = multiContent;
                            }

                            break;
                    }
                }

                if (content != null)
                    req.Content = content;

                var (client, isFactory) = CreateHttpClient(request);
                if (isFactory)
                {
                    var resp = await client.SendAsync(req);
                    return resp;
                }
                else
                {
                    using (client)
                    {
                        var resp = await client.SendAsync(req);
                        return resp;
                    }
                }
            }
            catch (Exception ex)
            {
                if (!(ex is SpearException))
                    _logger.LogError(ex, $"HttpHelper Request fail: {ex.Message}");

                throw;
            }
        }

        /// <summary> Get方法 </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<HttpResponseMessage> GetAsync(HttpRequest request)
        {
            return await RequestAsync(HttpMethod.Get, request);
        }

        /// <summary> Get方法 </summary>
        /// <param name="url"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public async Task<HttpResponseMessage> GetAsync(string url, object param = null)
        {
            return await RequestAsync(HttpMethod.Get, new HttpRequest(url) { Params = param });
        }

        /// <summary> Post方法 </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<HttpResponseMessage> PostAsync(HttpRequest request)
        {
            return await RequestAsync(HttpMethod.Post, request);
        }

        /// <summary> Post方法 </summary>
        /// <param name="url"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task<HttpResponseMessage> PostAsync(string url, object data = null)
        {
            return await RequestAsync(HttpMethod.Post, new HttpRequest(url) { Data = data });
        }

        /// <summary> Post方法 </summary>
        /// <param name="url"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task<HttpResponseMessage> FormPostAsync(string url, object data = null)
        {
            return await RequestAsync(HttpMethod.Post,
                new HttpRequest(url) { Data = data, BodyType = HttpBodyType.Form });
        }

        /// <summary> Post方法 </summary>
        /// <param name="url"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task<HttpResponseMessage> XmlPostAsync(string url, object data = null)
        {
            return await RequestAsync(HttpMethod.Post,
                new HttpRequest(url) { Data = data, BodyType = HttpBodyType.Xml });
        }
    }
}
