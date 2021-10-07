using System;

namespace Spear.Sdk.Core
{
    /// <summary> 配置接口 </summary>
    public interface ISdkConfig
    {
        /// <summary> 服务网关 </summary>
        string Gateway { get; set; }

        /// <summary> 服务编码 </summary>
        string Code { get; set; }

        /// <summary> 服务密钥 </summary>
        string Secret { get; set; }
    }

    /// <summary> 基础配置 </summary>
    [Serializable]
    public class SdkConfig : ISdkConfig
    {
        /// <summary> 服务网关 </summary>
        public string Gateway { get; set; }

        /// <summary> 服务编码 </summary>
        public string Code { get; set; }

        /// <summary> 服务密钥 </summary>
        public string Secret { get; set; }

        /// <summary> 异步通知地址 </summary>
        public string Notify { get; set; }
    }
}
