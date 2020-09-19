using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Spear.Core.EventBus
{
    /// <summary> 消息队列 </summary>
    public interface IMessageQueue
    {
        /// <summary> 发送消息 </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="message"></param>
        /// <param name="headers"></param>
        /// <returns></returns>
        Task Send<T>(T message, IDictionary<string, object> headers = null);

        /// <summary> 发送消息 </summary>
        /// <param name="message"></param>
        /// <param name="headers"></param>
        /// <returns></returns>
        Task Send(byte[] message, IDictionary<string, object> headers = null);

        /// <summary> 接收消息 </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        Task<T> Receive<T>();

        /// <summary> 批量接收消息 </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="size"></param>
        /// <returns></returns>
        Task<IEnumerable<T>> Receive<T>(int size);

        /// <summary> 订阅消息 </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="action"></param>
        /// <returns></returns>
        Task Subscibe<T>(Action<T> action);
    }
}
