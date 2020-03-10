using System.ComponentModel;

namespace Spear.Core.Exceptions
{
    public abstract class ErrorCodes
    {
        /// <summary> 默认错误码 </summary>
        public const int DefaultCode = -1;

        /// <summary> 系统开小差了，我们正在找他回来~ </summary>
        [Description("系统开小差了，我们正在找他回来~")]
        public const int SystemError = 10001;

        /// <summary> 参数错误 </summary>
        [Description("参数错误")]
        public const int ParamaterError = 10002;

        /// <summary> 调用受限 </summary>
        [Description("该请求调用受限")]
        public const int ClientError = 10003;

        /// <summary> 调用受限 </summary>
        [Description("该请求已超时")]
        public const int ClientTimeoutError = 10004;

        /// <summary> 需要客户端令牌 </summary>
        [Description("登录令牌无效")]
        public const int NeedTicket = 10005;

        /// <summary> 客户端令牌已失效 </summary>
        [Description("登录令牌已失效")]
        public const int InvalidTicket = 10006;

        /// <summary> 没有可用的服务 </summary>
        [Description("没有可用的服务")]
        public const int NoService = 10007;
    }
}
