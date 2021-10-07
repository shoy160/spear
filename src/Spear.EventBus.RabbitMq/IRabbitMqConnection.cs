using Spear.Core.Dependency;
using RabbitMQ.Client;
using System;

namespace Spear.RabbitMq
{
    /// <summary> MQ连接 </summary>
    public interface IRabbitMqConnection : IDisposable, IScopedDependency
    {
        string Name { get; }
        /// <summary> 交换机 </summary>
        string Broker { get; }

        /// <summary> 是否连接 </summary>
        bool IsConnected { get; }

        /// <summary> 尝试连接 </summary>
        /// <returns></returns>
        bool TryConnect();

        /// <summary> 创建RabbitMQ Model </summary>
        /// <returns></returns>
        IModel CreateModel();
    }
}
