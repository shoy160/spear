using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Spear.Core.Extensions
{
    public static class TaskExtensions
    {
        // .net core 已取消SynchronizationContext
        //private static IDisposable Enter()
        //{
        //    var context = SynchronizationContext.Current;
        //    SynchronizationContext.SetSynchronizationContext(null);
        //    return new Disposable(context);
        //}

        //private struct Disposable : IDisposable
        //{
        //    private readonly SynchronizationContext _synchronizationContext;

        //    public Disposable(SynchronizationContext synchronizationContext)
        //    {
        //        _synchronizationContext = synchronizationContext;
        //    }

        //    public void Dispose() =>
        //        SynchronizationContext.SetSynchronizationContext(_synchronizationContext);
        //}

        /// <summary> 同步执行异步方法(返回真实异常;.Result 异常是AggregateException) </summary>
        /// <param name="task"></param>
        public static void SyncRun(this Task task)
        {
            //using (Enter())
            {
                task.GetAwaiter().GetResult();
            }
        }

        /// <summary> 同步执行异步方法(返回真实异常;.Result 异常是AggregateException) </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="task"></param>
        /// <returns></returns>
        public static T SyncRun<T>(this Task<T> task)
        {
            //using (Enter())
            {
                return task.GetAwaiter().GetResult();
            }
        }

        /// <summary> 获取异步Task结果 </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        public static async Task<object> TaskResult(this object result)
        {
            if (result == null)
                return null;
            var resultType = result.GetType();
            if (resultType == typeof(void) || resultType == typeof(Task))
                return result;

            if (!(result is Task task))
                return result;
            await task;
            var taskType = task.GetType().GetTypeInfo();
            if (!taskType.IsGenericType) return result;
            var prop = taskType.GetProperty("Result");
            return prop?.GetValue(task);
        }

        /// <summary>
        /// 扩展Action支持async
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public static TaskAwaiter GetAwaiter(this Action action)
        {
            var task = Task.Run(action);
            return task.GetAwaiter();
        }
    }
}
