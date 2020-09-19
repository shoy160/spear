using Microsoft.Extensions.Logging;
using Spear.Core.Data;
using Spear.Core.Dependency;
using Spear.Core.Domain;
using Spear.Core.Domain.Entities;
using Spear.Core.Extensions;
using System;
using System.Data;
using System.Threading.Tasks;

namespace Spear.Dapper.Domain
{
    public abstract class DRepository : IDependency
    {
        /// <summary> 数据库连接提供者 </summary>
        public IUnitOfWork UnitOfWork { get; }
        /// <summary> 获取默认连接 </summary>
        protected IDbConnection Connection => UnitOfWork.Connection;

        /// <summary> 当前事务 </summary>
        protected IDbTransaction Trans => UnitOfWork.Transaction;

        /// <summary> 开启事务的连接或者当前连接 </summary>
        protected IDbConnection TransConnection => UnitOfWork.TransConnection;

        private readonly IDbConnectionProvider _connProvider;

        protected readonly ILogger Logger;

        protected DRepository(IUnitOfWork unitOfWork)
        {
            UnitOfWork = unitOfWork;
            _connProvider = CurrentIocManager.Resolve<IDbConnectionProvider>();
            Logger = CurrentIocManager.CreateLogger(GetType());
            if (Logger.IsEnabled(LogLevel.Debug))
                Logger.LogDebug($"{GetType().Name} Create");
        }


        /// <summary> 建议使用Ioc注入的方式 </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        [Obsolete("建议使用Ioc注入的方式")]
        public static T Instance<T>() where T : DRepository
        {
            return CurrentIocManager.Resolve<T>();
        }

        /// <summary> 获取数据库连接 </summary>
        /// <param name="connectionName"></param>
        /// <returns></returns>
        protected IDbConnection GetConnection(Enum connectionName)
        {
            return _connProvider.Connection(connectionName);
        }

        /// <summary> 获取数据库连接 </summary>
        /// <param name="connectionName"></param>
        /// <returns></returns>
        protected IDbConnection GetConnection(string connectionName = null)
        {
            return string.IsNullOrWhiteSpace(connectionName)
                ? UnitOfWork.CreateConnection()
                : _connProvider.Connection(connectionName);
        }

        /// <summary> 执行事务(当前连接) </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="action"></param>
        /// <param name="level"></param>
        /// <returns></returns>
        protected TResult Transaction<TResult>(Func<TResult> action, IsolationLevel? level = null)
        {
            return UnitOfWork.Trans(action, level);
        }

        /// <summary> 执行事务(当前连接) </summary>
        /// <param name="action"></param>
        /// <param name="level"></param>
        /// <returns></returns>
        protected Task Transaction(Func<Task> action, IsolationLevel? level = null)
        {
            return UnitOfWork.Trans(action, level);
        }

        /// <summary> 执行事务(当前连接) </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="action"></param>
        /// <param name="level"></param>
        /// <returns></returns>
        protected Task<TResult> Transaction<TResult>(Func<Task<TResult>> action, IsolationLevel? level = null)
        {
            return UnitOfWork.Trans(action, level);
        }

        /// <summary> 执行事务(当前连接) </summary>
        /// <param name="action"></param>
        /// <param name="level"></param>
        /// <returns></returns>
        protected void Transaction(Action action, IsolationLevel? level = null)
        {
            UnitOfWork.Trans(action, level);
        }

        /// <summary> 执行事务(新开连接) </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="action"></param>
        /// <param name="level"></param>
        /// <param name="connectionName"></param>
        /// <returns></returns>
        protected TResult Transaction<TResult>(Func<IDbConnection, IDbTransaction, TResult> action,
             string connectionName = null, IsolationLevel? level = null)
        {
            using (var conn = GetConnection(connectionName))
            {
                if (conn.State == ConnectionState.Closed)
                    conn.Open();
                using (var trans = level.HasValue ? conn.BeginTransaction(level.Value) : conn.BeginTransaction())
                {
                    try
                    {
                        var result = action.Invoke(conn, trans);
                        var task = result as Task;
                        task?.SyncRun();
                        trans.Commit();
                        return result;
                    }
                    catch
                    {
                        trans.Rollback();
                        throw;
                    }
                }
            }
        }

        /// <summary> 执行事务(新开连接) </summary>
        /// <param name="action"></param>
        /// <param name="level"></param>
        /// <param name="connectionName"></param>
        protected void Transaction(Action<IDbConnection, IDbTransaction> action, string connectionName = null,
            IsolationLevel? level = null)
        {
            Transaction((conn, trans) =>
            {
                action.Invoke(conn, trans);
                return true;
            }, connectionName, level);
        }

        /// <summary> 更新数量 </summary>
        /// <param name="column"></param>
        /// <param name="key"></param>
        /// <param name="keyColumn"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        protected int Increment<T>(string column, object key, string keyColumn = "id",
            int count = 1)
            where T : IEntity
        {
            return Connection.Increment<T>(column, key, keyColumn, count, Trans);
        }

        /// <summary> SELECT语句构建 </summary>
        /// <param name="where">where</param>
        /// <param name="orderby">order by</param>
        /// <param name="excepts">排除字段</param>
        /// <param name="includes">包含字段</param>
        /// <param name="tableAlias">表别名</param>
        /// <returns></returns>
        public string Select<TModel>(string where, string orderby = null, string[] excepts = null,
            string[] includes = null, string tableAlias = null)
            where TModel : IEntity
        {
            return typeof(TModel).Select(where, orderby, excepts, includes, tableAlias);
        }
    }
}
