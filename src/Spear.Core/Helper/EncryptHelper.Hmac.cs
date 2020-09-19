using System;
using System.Security.Cryptography;
using System.Text;

namespace Spear.Core.Helper
{
    /// <summary> 基于密钥的Hash加密 </summary>
    public static partial class EncryptHelper
    {
        /// <summary>
        /// 基于密钥的 Hash 加密采用的算法
        /// </summary>
        public enum HmacFormat
        {
            HMACMD5,
            //HMACRIPEMD160,
            HMACSHA1,
            HMACSHA256,
            HMACSHA384,
            HMACSHA512
        }

        /// <summary> 获取基于密钥的 Hash 加密方法 </summary>
        private static HMAC GetHmac(HmacFormat hmacFormat, byte[] key)
        {
            HMAC hmac;

            switch (hmacFormat)
            {
                case HmacFormat.HMACMD5:
                    hmac = new HMACMD5(key);
                    break;
                //case HmacFormat.HMACRIPEMD160:
                //    hmac = new HMACRIPEMD160(key);
                //    break;
                case HmacFormat.HMACSHA1:
                    hmac = new HMACSHA1(key);
                    break;
                case HmacFormat.HMACSHA256:
                    hmac = new HMACSHA256(key);
                    break;
                case HmacFormat.HMACSHA384:
                    hmac = new HMACSHA384(key);
                    break;
                case HmacFormat.HMACSHA512:
                    hmac = new HMACSHA512(key);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(hmacFormat), hmacFormat, null);
            }

            return hmac;
        }

        /// <summary> 对字符串进行基于密钥的 Hash 加密 </summary>
        /// <param name="inputString"></param>
        /// <param name="key">密钥的长度不限，建议的密钥长度为 64 个英文字符。</param>
        /// <param name="hashFormat"></param>
        /// <returns></returns>
        public static string Hmac(string inputString, string key, HmacFormat hashFormat = HmacFormat.HMACSHA1)
        {
            var algorithm = GetHmac(hashFormat, Encoding.ASCII.GetBytes(key));

            algorithm.ComputeHash(Encoding.UTF8.GetBytes(inputString));

            return BitConverter.ToString(algorithm.Hash).Replace("-", "").ToUpper();
        }
    }
}
