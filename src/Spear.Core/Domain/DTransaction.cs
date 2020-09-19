using System;
using System.Threading.Tasks;
using System.Transactions;
using Spear.Core.Extensions;

namespace Spear.Core.Domain
{
    /// <summary> 分布式事务辅助类 </summary>
    public class DTransaction : IDisposable
    {
        private readonly TransactionScope _transaction;

        private DTransaction(IsolationLevel level = IsolationLevel.ReadUncommitted, TimeSpan? timeout = null)
        {
            var options = new TransactionOptions
            {
                IsolationLevel = level
            };
            if (timeout.HasValue)
                options.Timeout = timeout.Value;
            _transaction = new TransactionScope(TransactionScopeOption.Required, options);
        }

        private void SaveChanges()
        {
            _transaction?.Complete();
        }

        public static TResult Use<TResult>(Func<TResult> action, IsolationLevel level = IsolationLevel.ReadUncommitted,
            TimeSpan? timeout = null)
        {
            using (var trans = new DTransaction(level, timeout))
            {
                var result = action();
                var task = result as Task;
                task?.SyncRun();
                trans.SaveChanges();
                return result;
            }
        }

        /// <summary> 使用分布式事务 </summary>
        /// <param name="action"></param>
        /// <param name="level"></param>
        /// <param name="timeout"></param>
        public static DResult Use(Action action, IsolationLevel level = IsolationLevel.ReadUncommitted,
            TimeSpan? timeout = null)
        {
            return Use(() =>
            {
                action();
                return DResult.Success;
            }, level, timeout);
        }

        public void Dispose()
        {
            _transaction?.Dispose();
        }
    }
}
