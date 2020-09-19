using Spear.Core.Helper;
using Spear.Core.Serialize;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Spear.Core.Extensions
{
    /// <summary> Http扩展 </summary>
    public static class HttpExtensions
    {
        /// <summary> 是否Gzip </summary>
        /// <param name="resp"></param>
        /// <returns></returns>
        public static bool IsGzip(this HttpResponseMessage resp)
        {
            return resp.Content.Headers.ContentEncoding.Contains("gzip");
        }

        ///// <summary> 是否GZip </summary>
        ///// <param name="context"></param>
        ///// <returns></returns>
        //public static bool IsGzip(this HttpContext context)
        //{
        //    return context.Response != null &&
        //           context.Response.Headers.TryGetValue("Content-Encoding", out var encoding) &&
        //           encoding.Contains("gzip");
        //}

        /// <summary> Read byte[] </summary>
        /// <param name="resp"></param>
        /// <returns></returns>
        public static async Task<byte[]> ReadAsBufferAsync(this HttpResponseMessage resp)
        {
            if (resp?.Content == null)
                return null;
            var buffers = await resp.Content.ReadAsByteArrayAsync();
            if (resp.IsGzip())
            {
                buffers = await buffers.UnZip();
            }

            return buffers;
        }

        /// <summary> fix gzip and encoding </summary>
        /// <param name="resp"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static async Task<string> ReadAsStringAsync(this HttpResponseMessage resp, Encoding encoding = null)
        {
            var buffers = await resp.ReadAsBufferAsync();
            if (buffers == null)
                return string.Empty;
            if (encoding == null)
            {
                var charset = resp.Content?.Headers?.ContentType?.CharSet;
                encoding = !string.IsNullOrWhiteSpace(charset) ? Encoding.GetEncoding(charset) : Encoding.UTF8;
            }
            return encoding.GetString(buffers);
        }

        /// <summary> Read T </summary>
        /// <param name="resp"></param>
        /// <param name="encoding"></param>
        /// <param name="def"></param>
        /// <returns></returns>
        public static async Task<T> ReadAsAsync<T>(this HttpResponseMessage resp, Encoding encoding = null,
            T def = default)
        {
            if (!resp.IsSuccessStatusCode)
                return def;
            var content = await resp.ReadAsStringAsync(encoding);
            if (string.IsNullOrWhiteSpace(content))
                return def;
            var contentType = resp.Content?.Headers?.ContentType?.MediaType;
            switch (contentType)
            {
                case "text/xml":
                case "application/xml":
                    return XmlHelper.XmlDeserialize(content, encoding, def);
            }
            return JsonHelper.Json<T>(content);
        }
    }
}
