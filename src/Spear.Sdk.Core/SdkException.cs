using System;

namespace Spear.Sdk.Core
{
    public class SdkException : Exception
    {
        /// <summary> 错误码 </summary>
        public int Code { get; }

        public SdkException(string message, int code = -1, Exception inner = null)
            : base(message, inner)
        {
            Code = code;
        }
    }
}
