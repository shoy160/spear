using System.Collections.Generic;

namespace Spear.Sdk.Core.Dtos
{
    public enum ContentType
    {
        Json,
        Xml,
        Form
    }

    public class SdkRequestDto
    {
        public string Api { get; set; }
        public string Method { get; set; } = "GET";

        public object Data { get; set; }
        public object Param { get; set; }

        public ContentType ContentType { get; set; } = ContentType.Json;
        public IDictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();

        public SdkRequestDto() { }

        public SdkRequestDto(string api, string method)
        {
            Api = api;
            Method = method;
        }
    }
}
