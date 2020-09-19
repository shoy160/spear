using System.Data;

namespace Spear.Core.Data
{
    public interface IActiveTransactionProvider
    {
        /// <summary> 获取当前事务 </summary>
        /// <param name="configName"></param>
        /// <returns></returns>
        IDbTransaction GetActiveTransaction(string configName = null);

        /// <summary> 获取当前连接 </summary>
        /// <param name="configName"></param>
        /// <returns></returns>
        IDbConnection GetActiveConnection(string configName = null);
    }
}
