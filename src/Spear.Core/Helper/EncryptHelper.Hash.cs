using System;
using System.Security.Cryptography;
using System.Text;

namespace Spear.Core.Helper
{
    /// <summary> Hash加密 </summary>
    public static partial class EncryptHelper
    {
        /// <summary> Hash 加密采用的算法 </summary>
        public enum HashFormat
        {
            MD516,
            MD532,
            //RIPEMD160,
            SHA1,
            SHA256,
            SHA384,
            SHA512
        }

        /// <summary> 获取 Hash 加密方法 </summary>
        private static HashAlgorithm GetHashAlgorithm(HashFormat hashFormat)
        {
            HashAlgorithm algorithm;

            switch (hashFormat)
            {
                case HashFormat.MD516:
                case HashFormat.MD532:
                    algorithm = System.Security.Cryptography.MD5.Create();
                    break;
                //case HashFormat.RIPEMD160:
                //    algorithm = RIPEMD160.Create();
                //    break;
                case HashFormat.SHA1:
                    algorithm = SHA1.Create();
                    break;
                case HashFormat.SHA256:
                    algorithm = SHA256.Create();
                    break;
                case HashFormat.SHA384:
                    algorithm = SHA384.Create();
                    break;
                case HashFormat.SHA512:
                    algorithm = SHA512.Create();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(hashFormat), hashFormat, null);
            }

            return algorithm;
        }

        /// <summary> 对字符串进行 Hash 加密 </summary>
        public static string Hash(string inputString, HashFormat hashFormat = HashFormat.SHA1)
        {
            var algorithm = GetHashAlgorithm(hashFormat);

            algorithm.ComputeHash(Encoding.UTF8.GetBytes(inputString));

            if (hashFormat == HashFormat.MD516)
                return BitConverter.ToString(algorithm.Hash).Replace("-", "").Substring(8, 16).ToUpper();

            return BitConverter.ToString(algorithm.Hash).Replace("-", "").ToUpper();
        }
    }
}
