using System;
using Acb.Core.Helper;
using Spear.Core.Message.Implementation;

namespace Spear.Core.Message
{
    public class MicroMessage : IMicroMessage
    {
        /// <summary> 消息ID </summary>
        public string Id { get; set; }
        /// <summary> 内容类型 </summary>
        public string ContentType { get; set; }
        /// <summary> 消息内容 </summary>
        public object Content { get; set; }

        public MicroMessage()
        {
        }

        public MicroMessage(object content, string contentType = null)
        {
            Id = IdentityHelper.Guid32;
            Content = content ?? throw new ArgumentNullException(nameof(content));
            contentType = string.IsNullOrWhiteSpace(contentType) ? content.GetType().FullName : contentType;
            ContentType = contentType;
        }

        /// <summary> 获取内容 </summary>
        /// <typeparam name="T">内容类型</typeparam>
        /// <returns>内容实例</returns>
        public T GetContent<T>()
        {
            return (T)Content;
        }

        /// <summary> 创建调用消息 </summary>
        /// <param name="invokeMessage"></param>
        /// <returns></returns>
        public static MicroMessage CreateInvokeMessage(InvokeMessage invokeMessage)
        {
            return new MicroMessage(invokeMessage, ContentTypes.InvokeType)
            {
                Id = Guid.NewGuid().ToString("N")
            };
        }

        /// <summary> 创建调用结果消息 </summary>
        /// <param name="id"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static MicroMessage CreateResultMessage(string id, ResultMessage result)
        {
            return new MicroMessage(result, ContentTypes.InvokeResultType)
            {
                Id = id
            };
        }
        /// <summary> 是否是调用消息 </summary>
        public bool IsInvoke => ContentType == ContentTypes.InvokeType;
        /// <summary> 是否是调用结果消息 </summary>
        public bool IsResult => ContentType == ContentTypes.InvokeResultType;
    }

    public static class MicroMessageExtension
    {
        /// <summary> 创建调用消息 </summary>
        /// <param name="invokeMessage"></param>
        /// <returns></returns>
        public static MicroMessage Create(this InvokeMessage invokeMessage) =>
            MicroMessage.CreateInvokeMessage(invokeMessage);

        /// <summary> 创建调用结果消息 </summary>
        /// <param name="invokeResultMessage"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static MicroMessage Create(this ResultMessage invokeResultMessage, string id) =>
            MicroMessage.CreateResultMessage(id, invokeResultMessage);
    }
}
