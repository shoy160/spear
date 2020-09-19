using System;
using System.Data;

namespace Spear.Core.Data
{
    public interface IDbConnectionAdapter
    {
        /// <summary> 适配器名称 </summary>
        string ProviderName { get; }

        /// <summary> 数据库连接类型 </summary>
        Type ConnectionType { get; }
        /// <summary> 格式化SQL </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        string FormatSql(string sql);

        /// <summary> 构造分页SQL </summary>
        /// <param name="sql"></param>
        /// <param name="columns"></param>
        /// <param name="order"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        string PageSql(string sql, string columns, string order, string count = null);
        /// <summary> 创建数据库连接 </summary>
        /// <returns></returns>
        IDbConnection Create();
    }
}
