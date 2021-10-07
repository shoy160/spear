using Spear.Sdk.Core.Dtos;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Spear.Sdk.Core.Helper
{
    /// <summary> 请求委托 </summary>
    /// <param name="req"></param>
    public delegate void BeforeRequestHandler(HttpWebRequest req);

    /// <summary> 执行结果事件 </summary>
    /// <param name="ex"></param>
    /// <returns></returns>
    public delegate Task RequestResultHandler(SdkRequestData ex);

    public class HttpHelper
    {
        private readonly string _gateway;

        /// <summary> 请求之前事件 </summary>
        public event BeforeRequestHandler OnRequest;

        /// <summary> 执行结果事件 </summary>
        public event RequestResultHandler OnResult;

        public HttpHelper(string gateway)
        {
            _gateway = gateway;
        }

        /// <summary> 默认请求头 </summary>
        private readonly IDictionary<string, string> _defaultHeaders = new Dictionary<string, string>
        {
            {"Accept-language", "zh-cn,zh;q=0.5" },
            {"Accept-Charset", "GB2312,utf-8;q=0.7,*;q=0.7" },
            {"Accept-Encoding", "gzip, deflate" },
            {"Keep-Alive", "350" },
            {"x-requested-with", "XMLHttpRequest" }
        };

        /// <summary> 发起请求 </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        public async Task<SdkResponseDto> RequestAsync(SdkRequestDto dto)
        {
            if (dto.Param != null)
            {
                var query = dto.Param.Stringfy(encoding: Encoding.UTF8);
                dto.Api += $"?{query}";
            }

            var uri = new Uri(new Uri(_gateway), dto.Api);
            var result = new SdkRequestData
            {
                Url = uri.AbsoluteUri,
                Method = dto.Method,
                Content = dto.Data,
                Headers = new Dictionary<string, string>()
            };
            HttpWebResponse resp = null;
            try
            {
                var req = (HttpWebRequest)WebRequest.Create(uri);
                OnRequest?.Invoke(req);
                req.AllowAutoRedirect = true;
                req.Method = dto.Method;
                req.Timeout = 150 * 1000;
                foreach (var header in _defaultHeaders)
                {
                    req.Headers.Add(header.Key, header.Value);
                }

                if (dto.Headers != null && dto.Headers.ContainsKey("UserAgent"))
                {
                    req.UserAgent = dto.Headers["UserAgent"];
                    dto.Headers.Remove("UserAgent");
                }
                else
                {
                    req.UserAgent = "SDK Request";
                }

                if (dto.Headers != null && dto.Headers.Any())
                {

                    foreach (var header in dto.Headers)
                    {
                        req.Headers.Add(header.Key, header.Value);
                    }
                }

                foreach (var key in req.Headers.AllKeys)
                {
                    if (!result.Headers.ContainsKey(key) && !_defaultHeaders.Keys.Contains(key))
                        result.Headers.Add(key, req.Headers.Get(key));
                }


                if (dto.Data != null)
                {
                    var reqStream = req.GetRequestStream();

                    switch (dto.ContentType)
                    {
                        case ContentType.Xml:
                            req.ContentType = "text/xml";
                            var serializer = new XmlSerializer(dto.Data.GetType());
                            serializer.Serialize(reqStream, dto.Data);
                            break;
                        case ContentType.Form:
                            req.ContentType = "application/x-www-form-urlencoded";
                            var content = dto.Data.Stringfy(false);
                            var buffer = Encoding.UTF8.GetBytes(content);
                            await reqStream.WriteAsync(buffer, 0, buffer.Length);
                            break;
                        default:
                            req.ContentType = "application/json";
                            var json = JsonHelper.ToJson(dto.Data);
                            var jsonBuffer = Encoding.UTF8.GetBytes(json);
                            await reqStream.WriteAsync(jsonBuffer, 0, jsonBuffer.Length);
                            break;
                    }
                }

                resp = (HttpWebResponse)req.GetResponse();
                var respDto = new SdkResponseDto
                {
                    Code = resp.StatusCode,
                    ContentType = resp.Headers["Content-Type"]
                };
                result.Code = (int)resp.StatusCode;
                var stream = resp.GetResponseStream();
                if (stream != null)
                {
                    if (string.Equals("gzip", resp.ContentEncoding, StringComparison.CurrentCultureIgnoreCase))
                        stream = new GZipStream(stream, CompressionMode.Decompress);
                    respDto.Content = stream;
                    result.Result = respDto.Result;
                }
                return respDto;
            }
            catch (Exception ex)
            {
                result.Exception = ex;
                throw;
            }
            finally
            {
                OnResult?.Invoke(result);
                resp.Dispose();
            }
        }

        public async Task<SdkResponseDto> PostAsync(string api, object data, IDictionary<string, string> headers = null)
        {
            return await RequestAsync(new SdkRequestDto(api, "POST")
            {
                Data = data,
                Headers = headers
            });
        }

        public async Task<SdkResponseDto> GetAsync(string api, object param = null,
            IDictionary<string, string> headers = null)
        {
            return await RequestAsync(new SdkRequestDto(api, "GET")
            {
                Param = param,
                Headers = headers
            });
        }

        public async Task<SdkResponseDto> PutAsync(string api, object param = null, object data = null,
            IDictionary<string, string> headers = null)
        {
            return await RequestAsync(new SdkRequestDto(api, "PUT")
            {
                Data = data,
                Param = param,
                Headers = headers
            });
        }

        public async Task<SdkResponseDto> DeleteAsync(string api, object param = null,
            IDictionary<string, string> headers = null)
        {
            return await RequestAsync(new SdkRequestDto(api, "DELETE")
            {
                Param = param,
                Headers = headers
            });
        }
    }
}
