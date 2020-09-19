using System;
using System.Text;

namespace Spear.Core.Tests
{
    /// <summary>
    /// 表示 <see cref="CodeTimer"/> 执行结果的类.
    /// </summary>
    public class CodeTimerResult
    {
        /// <summary>
        /// 初始化 <see cref="CodeTimer"/> 类的新实例.
        /// </summary>
        public CodeTimerResult()
        {
            GenerationList = new int[GC.MaxGeneration + 1];
        }

        /// <summary>
        /// 名称.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 运行时间.(ms)
        /// </summary>
        public long TimeElapsed { get; set; }

        /// <summary>
        /// Cpu 时钟周期(ns).ToString('N0')
        /// </summary>
        public long CpuCycles { get; set; }

        /// <summary>
        /// GC 代数集合.
        /// </summary>
        public int[] GenerationList { get; set; }

        /// <summary>
        /// 线程的计数.
        /// </summary>
        public int ThreadCount { get; set; }

        /// <summary>
        /// 重复的次数.
        /// </summary>
        public int Iteration { get; set; }

        /// <summary>
        /// 模拟思考的时间.
        /// </summary>
        public int MockThinkTime { get; set; }

        /// <summary>
        /// 执行成功计数.
        /// </summary>
        public int SuccessCount { get; set; }

        /// <summary>
        /// 执行失败计数.
        /// </summary>
        public int FailureCount { get; set; }

        /// <summary>
        /// 重置 <see cref="CodeTimer"/>.
        /// </summary>
        /// <returns>重置后的 <see cref="CodeTimer"/> 对象实例.</returns>
        public CodeTimerResult Reset()
        {
            Name = string.Empty;
            TimeElapsed = 0;
            CpuCycles = 0;
            GenerationList = new int[GC.MaxGeneration + 1];

            return this;
        }

        /// <summary> 格式化测试结果 </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var msg = new StringBuilder();
            msg.AppendLine(string.Concat("测试名称：", Name));
            msg.AppendLine(string.Concat("单线程重复的次数：", Iteration));
            msg.AppendLine(string.Concat("线程的计数：", ThreadCount));
            msg.AppendLine(string.Concat("模拟思考的时间：", MockThinkTime));
            msg.AppendLine(string.Concat("运行时间(ms)：", TimeElapsed));
            var tps = Math.Round((Iteration * ThreadCount * 1000D) / TimeElapsed, 3);
            msg.AppendLine(string.Concat("TPS(act/sec)：", tps));
            msg.AppendLine(string.Concat("Cpu 时钟周期(ns)：", CpuCycles.ToString("N0")));
            msg.AppendLine(string.Concat("执行成功计数：", SuccessCount));
            msg.AppendLine(string.Concat("执行失败计数：", FailureCount));
            return msg.ToString();
        }
    }
}
