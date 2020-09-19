using Spear.Core.Dependency;
using System;
using System.Data;

namespace Spear.Core.Data
{
    /// <summary> 数据库连接提供者接口 </summary>
    public interface IDbConnectionProvider : ISingleDependency
    {
        /// <summary> 获取数据库连接 </summary>
        /// <param name="connectionName">连接名称</param>
        IDbConnection Connection(string connectionName = null);

        /// <summary> 获取数据库连接 </summary>
        /// <param name="connectionName">连接名称</param>
        IDbConnection Connection(Enum connectionName);

        /// <summary> 创建数据库连接 </summary>
        /// <param name="connectionString"></param>
        /// <param name="providerName"></param>
        /// <returns></returns>
        IDbConnection Connection(string connectionString, string providerName);
    }
}
