using Spear.Core.Dependency;
using System;
using System.Data;
using System.Threading.Tasks;

namespace Spear.Core.Domain
{
    /// <summary> 业务单元操作接口 </summary>
    public interface IUnitOfWork : IScopedDependency, IDisposable
    {
        Guid Id { get; }

        /// <summary> 获取当前连接 </summary>
        IDbConnection Connection { get; }

        /// <summary> 创建新连接 </summary>
        IDbConnection CreateConnection();

        /// <summary> 获取当前事务 </summary>
        IDbTransaction Transaction { get; }

        /// <summary> 开启事务的连接或者当前连接 </summary>
        IDbConnection TransConnection { get; }

        /// <summary> 是否开启了事务 </summary>
        bool IsTransaction { get; }

        /// <summary> 开启事务 </summary>
        /// <param name="level"></param>
        bool Begin(IsolationLevel? level = null);

        /// <summary> 提交事务 </summary>
        void Commit();

        /// <summary> 回滚事务 </summary>
        void Rollback();
    }

    public static class UnitOfWorkExtensions
    {
        /// <summary> 开启事务 </summary>
        /// <param name="unitOfWork"></param>
        /// <param name="action"></param>
        /// <param name="level"></param>
        public static void Trans(this IUnitOfWork unitOfWork, Action action, IsolationLevel? level = null)
        {
            var trans = unitOfWork.Begin(level);
            try
            {
                action.Invoke();
                if (trans)
                    unitOfWork.Commit();
            }
            catch
            {
                if (trans)
                    unitOfWork.Rollback();
                throw;
            }
        }

        /// <summary> 开启事务 </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="unitOfWork"></param>
        /// <param name="action"></param>
        /// <param name="level"></param>
        /// <returns></returns>
        public static T Trans<T>(this IUnitOfWork unitOfWork, Func<T> action, IsolationLevel? level = null)
        {
            var trans = unitOfWork.Begin(level);
            try
            {
                var result = action.Invoke();
                if (trans)
                    unitOfWork.Commit();
                return result;
            }
            catch
            {
                if (trans)
                    unitOfWork.Rollback();
                throw;
            }
        }

        public static async Task Trans(this IUnitOfWork unitOfWork, Func<Task> action, IsolationLevel? level = null)
        {
            var trans = unitOfWork.Begin(level);
            try
            {
                await action.Invoke();
                if (trans)
                    unitOfWork.Commit();
            }
            catch
            {
                if (trans)
                    unitOfWork.Rollback();
                throw;
            }
        }

        public static async Task<T> Trans<T>(this IUnitOfWork unitOfWork, Func<Task<T>> action, IsolationLevel? level = null)
        {
            var trans = unitOfWork.Begin(level);
            try
            {
                var result = await action.Invoke();
                if (trans)
                    unitOfWork.Commit();
                return result;
            }
            catch
            {
                if (trans)
                    unitOfWork.Rollback();
                throw;
            }
        }
    }
}
