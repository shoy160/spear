using System;
using System.Collections.Generic;

namespace Spear.Sdk.Core.Dtos
{
    /// <summary> 请求数据 </summary>
    [Serializable]
    public class SdkRequestData
    {
        /// <summary> 请求URL </summary>
        public string Url { get; set; }

        /// <summary> 请求方法 </summary>
        public string Method { get; set; }

        /// <summary> 内容 </summary>
        public object Content { get; set; }

        /// <summary> 请求结果 </summary>
        public string Result { get; set; }

        /// <summary> 状态码 </summary>
        public int Code { get; set; }

        /// <summary> 请求头 </summary>
        public IDictionary<string, string> Headers { get; set; }

        /// <summary> 异常 </summary>
        public Exception Exception { get; set; }

    }
}
