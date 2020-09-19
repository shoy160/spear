using Spear.Core.Extensions;
using Spear.Core.Timing;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Spear.WebApi.ViewModels
{
    /// <inheritdoc />
    /// <summary> 参数签名基类 </summary>
    public class VSignature : IValidatableObject
    {
        protected const string SignConfigKey = "app_sign_key";

        private static string SignKey => SignConfigKey.Config<string>();

        /// <summary> 签名 </summary>
        [Required(ErrorMessage = "接口签名无效")]
        [StringLength(45, ErrorMessage = "接口签名无效")]
        public string Sign { get; set; }

        private static string UnEscape(object value)
        {
            if (value == null)
                return string.Empty;
            var type = value.GetType();
            //枚举值
            if (type.IsEnum)
                return value.CastTo(0).ToString();
            //布尔值
            if (type == typeof(bool))
                return ((bool)value ? 1 : 0).ToString();

            var sb = new StringBuilder();
            var str = value.ToString();
            var len = str.Length;
            var i = 0;
            while (i != len)
            {
                sb.Append(Uri.IsHexEncoding(str, i) ? Uri.HexUnescape(str, ref i) : str[i++]);
            }

            return sb.ToString();
        }

        protected virtual bool VerifyTimestamp(long timestamp)
        {
            //有效期验证 2分钟有效期
            return Clock.Now <= timestamp.FromMillisecondTimestamp().AddMinutes(2);
        }

        protected virtual bool VerifySign(object instance)
        {
            var timestamp = Sign.Substring(0, 13).CastTo(0L);
            //签名验证
            //获取除Sign外所有参数并进行排序、url拼接
            var props = GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var dict = props.Where(prop => prop.Name != nameof(Sign))
                .ToDictionary(prop => prop.Name, prop => prop.GetValue(instance));
            var array = new SortedDictionary<string, object>(dict).Select(t => $"{t.Key}={UnEscape(t.Value)}");

            // 规则 Sign=时间戳(毫秒) + Md532(除Sign外所有参数的url拼接(如：a=1&b=2,不编码) + key + 时间戳(毫秒)).ToLower()
            var unSigned = string.Concat(string.Join("&", array), SignKey, timestamp);
            var sign = timestamp + Core.Helper.EncryptHelper.MD5(unSigned);
            Console.WriteLine(sign);
            //签名验证
            return string.Equals(sign, Sign, StringComparison.CurrentCultureIgnoreCase);
        }

        /// <inheritdoc />
        /// <summary> 参数验证 </summary>
        /// <param name="validationContext"></param>
        /// <returns></returns>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (string.IsNullOrWhiteSpace(SignKey))
                yield break;
            if (Sign.Length != 45)
                yield return new ValidationResult("参数签名格式异常");
            else
            {
                var timestamp = Sign.Substring(0, 13).CastTo(0L);
                if (!VerifyTimestamp(timestamp))
                {
                    yield return new ValidationResult("请求已失效");
                }
                else
                {
                    if (!VerifySign(validationContext.ObjectInstance))
                        yield return new ValidationResult("参数签名验证失败");
                }
            }
        }
    }
}
