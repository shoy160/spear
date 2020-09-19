using System;
using System.Data;

namespace Spear.Dapper
{
    /// <summary> 连接管理类 </summary>
    internal class ConnectionStruct : IDisposable
    {
        private int _useCount;
        private readonly IDbConnection _connection;
        private readonly DateTime _createTime;
        private DateTime _lasteUsedTime;
        private readonly TimeSpan _aliveTime;

        /// <summary> 构造函数 </summary>
        /// <param name="connection">数据库连接</param>
        /// <param name="createTime">创建时间</param>
        /// <param name="aliveTime">存活时间</param>
        public ConnectionStruct(IDbConnection connection, DateTime? createTime = null, TimeSpan? aliveTime = null)
        {
            _connection = connection;
            _createTime = _lasteUsedTime = (createTime ?? DateTime.Now);
            _useCount = 0;
            _aliveTime = aliveTime ?? TimeSpan.FromMinutes(30);
        }

        /// <summary> 获取连接 </summary>
        /// <returns></returns>
        public IDbConnection GetConnection()
        {
            _useCount++;
            _lasteUsedTime = DateTime.Now;
            return _connection;
        }

        /// <summary> 是否存活 </summary>
        /// <returns></returns>
        public bool IsAlive()
        {
            return (DateTime.Now - _lasteUsedTime) < _aliveTime;
        }

        /// <summary> 连接信息 </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var time = _aliveTime - (DateTime.Now - _lasteUsedTime);
            return
                $"{_connection.Database}_{_connection.GetHashCode()},{_createTime:MM-dd HH:mm},{_lasteUsedTime:MM-dd HH:mm},{_useCount}次,{time.TotalMilliseconds}ms";
        }

        public void Dispose()
        {
            _connection?.Dispose();
        }
    }
}
