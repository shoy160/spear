using StackExchange.Redis;
using System;
using System.Net;

namespace Spear.Redis
{
    public interface IRedisConnection : IDisposable
    {
        ConnectionMultiplexer Connection { get; }
        IDatabase GetDatabase(int db = -1, object asyncState = null);
        IServer GetServer(EndPoint endPoint, object asyncState = null);

        ISubscriber GetSubscriber();
    }
}
