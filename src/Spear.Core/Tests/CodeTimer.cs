using Microsoft.Extensions.Logging;
using Spear.Core.Dependency;
using Spear.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Spear.Core.Tests
{
    /// <summary>
    /// 代码性能测试计时器
    /// </summary>
    public static class CodeTimer
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool GetThreadTimes(IntPtr hThread, out long lpCreationTime,
                                          out long lpExitTime, out long lpKernelTime, out long lpUserTime);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetCurrentThread();

        private static ILogger Logger => CurrentIocManager.CreateLogger(typeof(CodeTimer));

        private static long GetCurrentThreadTimes()
        {
            GetThreadTimes(GetCurrentThread(), out long _, out _, out var kernelTime,
                           out var userTimer);
            return kernelTime + userTimer;
        }

        static CodeTimer()
        {
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.High;
            Thread.CurrentThread.Priority = ThreadPriority.Highest;

        }

        public static async Task<CodeTimerResult> Time(string name, int iteration, Func<Task> action, int thread = 1)
        {
            if (string.IsNullOrEmpty(name) || action == null)
            {
                return null;
            }

            var result = new CodeTimerResult();
            result = result.Reset();
            result.Name = name;
            result.Iteration = iteration;
            result.ThreadCount = thread;

            GC.Collect(GC.MaxGeneration);
            var gcCounts = new int[GC.MaxGeneration + 1];
            for (var i = 0; i <= GC.MaxGeneration; i++)
            {
                gcCounts[i] = GC.CollectionCount(i);
            }

            // 3. Run action
            var watch = new Stopwatch();
            watch.Start();
            var ticksFst = GetCurrentThreadTimes(); //100 nanosecond one tick
            if (thread > 1)
            {
                var tasks = new List<Task>();
                for (var i = 0; i < thread; i++)
                {
                    var task = await Task.Factory.StartNew(async () =>
                    {
                        int succ = 0, fail = 0;
                        for (var j = 0; j < iteration; j++)
                        {
                            try
                            {
                                await action.Invoke();
                                succ++;
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.Format());
                                fail++;
                            }
                        }

                        return new KeyValuePair<int, int>(succ, fail);
                    });
                    tasks.Add(task);
                }

                Task.WaitAll(tasks.ToArray());
                foreach (var task1 in tasks)
                {
                    var task = (Task<KeyValuePair<int, int>>)task1;
                    result.SuccessCount += task.Result.Key;
                    result.FailureCount += task.Result.Value;
                }
            }
            else
            {
                try
                {
                    for (var i = 0; i < iteration; i++)
                    {
                        try
                        {
                            await action.Invoke();
                            result.SuccessCount++;
                        }
                        catch
                        {
                            result.FailureCount++;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, ex.Message);
                }
            }

            var ticks = GetCurrentThreadTimes() - ticksFst;
            watch.Stop();

            // 4. Print CPU
            result.TimeElapsed = watch.ElapsedMilliseconds;
            result.CpuCycles = ticks * 100;

            // 5. Print GC
            for (var i = 0; i <= GC.MaxGeneration; i++)
            {
                var count = GC.CollectionCount(i) - gcCounts[i];
                result.GenerationList[i] = count;
            }
            return result;
        }

        /// <summary> 代码耗时测试 </summary>
        /// <param name="name">测试名称</param>
        /// <param name="iteration">重复次数</param>
        /// <param name="action">执行方法</param>
        /// <param name="thread">线程数</param>
        /// <returns></returns>
        public static CodeTimerResult Time(string name, int iteration, Action action, int thread = 1)
        {
            if (string.IsNullOrEmpty(name) || action == null)
            {
                return null;
            }

            var result = new CodeTimerResult();
            result = result.Reset();
            result.Name = name;
            result.Iteration = iteration;
            result.ThreadCount = thread;

            GC.Collect(GC.MaxGeneration);
            var gcCounts = new int[GC.MaxGeneration + 1];
            for (var i = 0; i <= GC.MaxGeneration; i++)
            {
                gcCounts[i] = GC.CollectionCount(i);
            }

            // 3. Run action
            var watch = new Stopwatch();
            watch.Start();
            var ticksFst = GetCurrentThreadTimes(); //100 nanosecond one tick
            if (thread > 1)
            {
                var tasks = new List<Task>();
                for (var i = 0; i < thread; i++)
                {
                    var task = Task.Factory.StartNew(() =>
                    {
                        int succ = 0, fail = 0;
                        for (var j = 0; j < iteration; j++)
                        {
                            try
                            {
                                action.Invoke();
                                succ++;
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.Format());
                                fail++;
                            }
                        }

                        return new KeyValuePair<int, int>(succ, fail);
                    });
                    tasks.Add(task);
                }

                Task.WaitAll(tasks.ToArray());
                foreach (var task1 in tasks)
                {
                    var task = (Task<KeyValuePair<int, int>>)task1;
                    result.SuccessCount += task.Result.Key;
                    result.FailureCount += task.Result.Value;
                }
            }
            else
            {
                try
                {
                    for (var i = 0; i < iteration; i++)
                    {
                        try
                        {
                            action.Invoke();
                            result.SuccessCount++;
                        }
                        catch
                        {
                            result.FailureCount++;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, ex.Message);
                }
            }

            var ticks = GetCurrentThreadTimes() - ticksFst;
            watch.Stop();

            // 4. Print CPU
            result.TimeElapsed = watch.ElapsedMilliseconds;
            result.CpuCycles = ticks * 100;

            // 5. Print GC
            for (var i = 0; i <= GC.MaxGeneration; i++)
            {
                var count = GC.CollectionCount(i) - gcCounts[i];
                result.GenerationList[i] = count;
            }
            return result;
        }

        /// <summary> 使用案例 StringBuilder vs contact </summary>
        /// <returns></returns>
        public static string UseTest()
        {
            var result1 = Time("contact", 1000 * 200, () =>
              {
                  var str = "";
                  for (var i = 0; i < 10; i++)
                  {
                      str += "dddddddddddddddddddddd";
                  }
              });
            var result2 = Time("stringbuilder", 1000 * 200, () =>
              {
                  var sb = new System.Text.StringBuilder();
                  for (var i = 0; i < 10; i++)
                  {
                      sb.Append("dddddddddddddddddddddd");
                  }
              });
            return result1 + result2.ToString();
        }
    }
}