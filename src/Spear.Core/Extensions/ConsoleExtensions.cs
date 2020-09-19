using System;
using System.Collections.Generic;
using Spear.Core.Serialize;

namespace Spear.Core.Extensions
{
    /// <summary> 打印对象 </summary>
    public class PrintItem
    {
        /// <summary> 消息体 </summary>
        public object Message { get; set; }
        /// <summary> 打印颜色 </summary>
        public ConsoleColor? Color { get; set; }
        /// <summary> 是否新行 </summary>
        public bool NewLine { get; set; }

        /// <summary> Ctrl </summary>
        /// <param name="msg"></param>
        /// <param name="color"></param>
        /// <param name="newline"></param>
        public PrintItem(object msg, ConsoleColor? color = null, bool newline = true)
        {
            Message = msg;
            Color = color;
            NewLine = newline;
        }
    }

    /// <summary> 控制台扩展 </summary>
    public static class ConsoleExtensions
    {
        private static readonly object ConsoleSync = new object();

        /// <summary> 打印对象 </summary>
        /// <param name="prints"></param>
        public static void Print(this IEnumerable<PrintItem> prints)
        {
            lock (ConsoleSync)
            {
                foreach (var item in prints)
                {
                    ConsoleColor? tc = null;
                    if (item.Color.HasValue)
                    {
                        tc = Console.ForegroundColor;
                        Console.ForegroundColor = item.Color.Value;
                    }

                    var content = item.Message == null
                        ? "NULL"
                        : (item.Message.GetType().IsSimpleType()
                            ? item.Message.ToString()
                            : JsonHelper.ToJson(item.Message, NamingType.CamelCase, true));
                    if (item.NewLine)
                        Console.WriteLine(content);
                    else
                        Console.Write(content);
                    if (tc.HasValue)
                        Console.ForegroundColor = tc.Value;
                }
            }
        }

        /// <summary> 打印对象 </summary>
        /// <param name="msg"></param>
        /// <param name="color"></param>
        /// <param name="newline"></param>
        public static void Print(this object msg, ConsoleColor? color = null, bool newline = true)
        {
            new[] { new PrintItem(msg, color, newline) }.Print();
        }

        /// <summary> 打印成功 </summary>
        /// <param name="msg"></param>
        /// <param name="newline"></param>
        public static void PrintSucc(this object msg, bool newline = true)
        {
            msg.Print(ConsoleColor.Green, newline);
        }

        /// <summary> 打印消息 </summary>
        /// <param name="msg"></param>
        /// <param name="newline"></param>
        public static void PrintInfo(this object msg, bool newline = true)
        {
            msg.Print(ConsoleColor.Cyan, newline);
        }

        /// <summary> 打印警告 </summary>
        /// <param name="msg"></param>
        /// <param name="newline"></param>
        public static void PrintWarn(this object msg, bool newline = true)
        {
            msg.Print(ConsoleColor.Yellow, newline);
        }

        /// <summary> 打印错误 </summary>
        /// <param name="msg"></param>
        /// <param name="newline"></param>
        public static void PrintError(this object msg, bool newline = true)
        {
            msg.Print(ConsoleColor.Red, newline);
        }

        /// <summary> 打印致命错误 </summary>
        /// <param name="msg"></param>
        /// <param name="newline"></param>
        public static void PrintFatal(this object msg, bool newline = true)
        {
            msg.Print(ConsoleColor.Magenta, newline);
        }
    }
}
