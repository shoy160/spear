using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Spear.Core.Extensions;

namespace Spear.Core.Helper
{
    /// <summary> 对称加密算法 </summary>
    public static partial class EncryptHelper
    {
        /// <summary> 对称加密采用的算法 </summary>
        public enum SymmetricFormat
        {
            /// <summary>
            /// 有效的 KEY 与 IV 长度，以英文字符为单位： KEY（Min:8 Max:8 Skip:0），IV（8）
            /// </summary>
            DES,
            /// <summary>
            /// 有效的 KEY 与 IV 长度，以英文字符为单位： KEY（Min:16 Max:24 Skip:8），IV（8）
            /// </summary>
            TripleDES,
            /// <summary>
            /// 有效的 KEY 与 IV 长度，以英文字符为单位： KEY（Min:5 Max:16 Skip:1），IV（8）
            /// </summary>
            RC2,
            /// <summary>
            /// 有效的 KEY 与 IV 长度，以英文字符为单位： KEY（Min:16 Max:32 Skip:8），IV（16）
            /// </summary>
            Rijndael,
            /// <summary>
            /// 有效的 KEY 与 IV 长度，以英文字符为单位： KEY（Min:16 Max:32 Skip:8），IV（16）
            /// </summary>
            AES
        }

        /// <summary> 获取对称加密方法 </summary>
        private static SymmetricAlgorithm GetSymmetricAlgorithm(SymmetricFormat symmetricFormat)
        {
            SymmetricAlgorithm algorithm;

            switch (symmetricFormat)
            {
                case SymmetricFormat.DES:
                    algorithm = DES.Create();
                    break;
                case SymmetricFormat.TripleDES:
                    algorithm = TripleDES.Create();
                    break;
                case SymmetricFormat.RC2:
                    algorithm = RC2.Create();
                    break;
                case SymmetricFormat.Rijndael:
                    algorithm = Rijndael.Create();
                    break;
                case SymmetricFormat.AES:
                    algorithm = Aes.Create();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(symmetricFormat), symmetricFormat, null);
            }

            return algorithm;
        }

        /// <summary> 对字符串进行对称加密 </summary>
        public static string SymmetricEncrypt(string inputString, SymmetricFormat symmetricFormat, string key,
            string iv)
        {
            var algorithm = GetSymmetricAlgorithm(symmetricFormat);

            var desString = Encoding.UTF8.GetBytes(inputString);

            var desKey = Encoding.ASCII.GetBytes(key);

            var desIv = Encoding.ASCII.GetBytes(iv);

            if (!algorithm.ValidKeySize(desKey.Length * 8))
                throw new ArgumentOutOfRangeException(nameof(key));

            if (algorithm.IV.Length != desIv.Length)
                throw new ArgumentOutOfRangeException(nameof(iv));

            var mStream = new MemoryStream();

            var cStream = new CryptoStream(mStream, algorithm.CreateEncryptor(desKey, desIv), CryptoStreamMode.Write);

            cStream.Write(desString, 0, desString.Length);

            cStream.FlushFinalBlock();

            cStream.Close();

            return Convert.ToBase64String(mStream.ToArray());
        }

        /// <summary> 对字符串进行对称解密 </summary>
        public static string SymmetricDecrypt(string inputString, SymmetricFormat symmetricFormat, string key,
            string iv)
        {
            if (string.IsNullOrWhiteSpace(inputString))
                return inputString;
            inputString = inputString.CleanBase64();
            if (!inputString.IsBase64())
                return string.Empty;

            var algorithm = GetSymmetricAlgorithm(symmetricFormat);

            var desString = Convert.FromBase64String(inputString);

            var desKey = Encoding.ASCII.GetBytes(key);

            var desIv = Encoding.ASCII.GetBytes(iv);

            var mStream = new MemoryStream();

            var cStream = new CryptoStream(mStream, algorithm.CreateDecryptor(desKey, desIv), CryptoStreamMode.Write);

            cStream.Write(desString, 0, desString.Length);

            cStream.FlushFinalBlock();

            cStream.Close();

            return Encoding.UTF8.GetString(mStream.ToArray());
        }
    }
}
