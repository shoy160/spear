using System;
using System.Collections.Generic;
using System.Linq;

namespace Spear.Sdk.Core.Dtos
{
    [Serializable]
    public class SdkResult
    {
        /// <summary> 状态 </summary>
        public bool Status => Code == 0;
        /// <summary> 状态码 </summary>
        public int Code { get; set; }
        /// <summary> 错误消息 </summary>
        public string Message { get; set; }

        public SdkResult() { }

        public SdkResult(string message, int code = -1)
        {
            Message = message;
            Code = code;
        }
    }

    [Serializable]
    public class SdkResult<T> : SdkResult
    {
        public T Data { get; set; }
    }

    [Serializable]
    public class SdkResults<T> : SdkResult
    {
        /// <summary> 数据集合 </summary>
        public IEnumerable<T> Data { get; set; }

        /// <summary> 总数 </summary>
        public int Total { get; set; }

        public SdkResults(IEnumerable<T> data, int? count = null)
        {
            var array = data as T[] ?? data.ToArray();
            Data = array;
            Total = count ?? array.Length;
        }
    }
}
