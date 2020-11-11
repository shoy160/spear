using Spear.Core.Extensions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace Spear.Core.Exceptions
{
    /// <summary> 错误码扩展 </summary>
    public static class ErrorCodesExtension
    {
        private static readonly ConcurrentDictionary<RuntimeTypeHandle, IDictionary<int, string>> ErrorCodesCache =
            new ConcurrentDictionary<RuntimeTypeHandle, IDictionary<int, string>>();

        private static readonly Type ErrorType = typeof(ErrorCodes);

        /// <summary> 获取错误码对应的错误信息 </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public static string Message(this int code)
        {
            return code.Message<ErrorCodes>();
        }

        /// <summary> 获取错误码对应的错误信息 </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public static string Message<T>(this int code) where T : ErrorCodes
        {
            var message = $"errorMsg:{code}".Config<string>();
            if (message != null)
                return message;
            var codes = Codes<T>();
            return codes.TryGetValue(code, out message) ? message : ErrorCodes.SystemError.Message<ErrorCodes>();
        }

        /// <summary> 错误编码对应DResult </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="code"></param>
        /// <returns></returns>
        public static DResult CodeResult<T>(this int code) where T : ErrorCodes
        {
            return DResult.Error(code.Message<T>(), code);
        }

        /// <summary> 错误编码对应DResult </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public static DResult CodeResult(this int code)
        {
            return code.CodeResult<ErrorCodes>();
        }

        /// <summary> 错误编码对应的Exception </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="code"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static BusiException CodeException<T>(this int code, string message = null) where T : ErrorCodes
        {
            message = string.IsNullOrWhiteSpace(message) ? code.Message<T>() : message;
            return new BusiException(message, code);
        }

        /// <summary> 错误编码对应的Exception </summary>
        /// <param name="code"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static BusiException CodeException(this int code, string message = null)
        {
            return code.CodeException<ErrorCodes>(message);
        }

        /// <summary> 获取错误码 </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IDictionary<int, string> Codes<T>() where T : ErrorCodes
        {
            return typeof(T).Codes();
        }

        /// <summary> 获取错误码 </summary>
        /// <returns></returns>
        public static IDictionary<int, string> Codes()
        {
            return typeof(ErrorCodes).Codes();
        }

        /// <summary> 获取错误码 </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static IDictionary<int, string> Codes(this Type type)
        {
            if (type != ErrorType && !type.IsSubclassOf(ErrorType))
                return new Dictionary<int, string>();
            var key = type.TypeHandle;
            if (ErrorCodesCache.ContainsKey(key) && ErrorCodesCache.TryGetValue(type.TypeHandle, out var codes))
                return codes;
            codes = new Dictionary<int, string>();
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
            foreach (var field in fields)
            {
                var message = field.GetCustomAttribute<DescriptionAttribute>()?.Description ?? field.Name;
                codes.Add(field.GetRawConstantValue().CastTo<int>(), message);
            }

            while (type != null && type != ErrorType)
            {
                type = type.BaseType;
                foreach (var t in type.Codes())
                {
                    codes.Add(t.Key, t.Value);
                }
            }

            var orderCodes = codes.OrderBy(t => t.Key).ToDictionary(k => k.Key, v => v.Value);
            ErrorCodesCache.TryAdd(key, orderCodes);
            return orderCodes;
        }
    }
}
