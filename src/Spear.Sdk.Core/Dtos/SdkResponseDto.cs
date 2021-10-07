using Spear.Sdk.Core.Helper;
using System.IO;
using System.Net;

namespace Spear.Sdk.Core.Dtos
{
    public class SdkResponseDto
    {
        public HttpStatusCode Code { get; set; }
        public Stream Content { get; set; }
        public string ContentType { get; set; }

        private string _result;

        public string Result
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(_result))
                    return _result;
                if (Content == null) return string.Empty;
                using (var reader = new StreamReader(Content))
                {
                    var result = reader.ReadToEndAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                    if (Content.CanSeek)
                        Content.Seek(0, SeekOrigin.Begin);
                    return _result = result;
                }
            }
        }

        public T Get<T>()
        {
            return JsonHelper.Json<T>(Result);
        }
    }
}
