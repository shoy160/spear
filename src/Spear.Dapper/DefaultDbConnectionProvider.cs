using Spear.Core.Data;
using System;
using System.Data;

namespace Spear.Dapper
{
    /// <summary> 数据库连接管理 </summary>
    public class DefaultDbConnectionProvider : IDbConnectionProvider
    {
        private readonly ConnectionFactory _factory;

        public DefaultDbConnectionProvider(ConnectionFactory factory)
        {
            _factory = factory;
        }

        /// <summary> 获取数据库连接 </summary>
        /// <param name="connectionName">连接名称</param>
        /// <returns></returns>
        public IDbConnection Connection(string connectionName = null)
        {
            return _factory.Connection(connectionName, false);
        }

        /// <summary> 获取数据库连接 </summary>
        /// <param name="connectionName">连接名称</param>
        /// <returns></returns>
        public IDbConnection Connection(Enum connectionName)
        {
            return _factory.Connection(connectionName, false);
        }

        public IDbConnection Connection(string connectionString, string providerName)
        {
            return _factory.Connection(connectionString, providerName, false);
        }
    }
}
