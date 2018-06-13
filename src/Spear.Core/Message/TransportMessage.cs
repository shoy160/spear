using System;

namespace Spear.Core.Message
{
    /// <summary> 传输消息实体 </summary>
    public class TransportMessage
    {
        /// <summary> 消息Id </summary>
        public string Id { get; set; }

        /// <summary> 消息内容 </summary>
        public object Content { get; set; }

        /// <summary> 内容类型 </summary>
        public string ContentType { get; set; }

        public TransportMessage()
        {
        }

        public TransportMessage(object content)
        {
            Content = content ?? throw new ArgumentNullException(nameof(content));
            ContentType = content.GetType().FullName;
        }

        public TransportMessage(object content, string contentType)
        {
            Content = content ?? throw new ArgumentNullException(nameof(content));
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
        public static TransportMessage CreateInvokeMessage(InvokeMessage invokeMessage)
        {
            return new TransportMessage(invokeMessage, ContentTypes.InvokeType)
            {
                Id = Guid.NewGuid().ToString("N")
            };
        }

        /// <summary> 创建调用结果消息 </summary>
        /// <param name="id"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static TransportMessage CreateInvokeResultMessage(string id, InvokeResultMessage result)
        {
            return new TransportMessage(result, ContentTypes.InvokeResultType)
            {
                Id = id
            };
        }
        /// <summary> 是否是调用消息 </summary>
        public bool IsInvoke => ContentType == ContentTypes.InvokeType;
        /// <summary> 是否是调用结果消息 </summary>
        public bool IsResult => ContentType == ContentTypes.InvokeResultType;
    }

    public static class TransportMessageExtension
    {
        /// <summary> 创建调用消息 </summary>
        /// <param name="invokeMessage"></param>
        /// <returns></returns>
        public static TransportMessage Create(this InvokeMessage invokeMessage) =>
            TransportMessage.CreateInvokeMessage(invokeMessage);

        /// <summary> 创建调用结果消息 </summary>
        /// <param name="invokeResultMessage"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static TransportMessage Create(this InvokeResultMessage invokeResultMessage, string id) =>
            TransportMessage.CreateInvokeResultMessage(id, invokeResultMessage);
    }
}
