using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Spear.Core.Helper
{
    /// <summary> 加密和解密 </summary>
    public static partial class EncryptHelper
    {
        /// <summary> RSA类型 </summary>
        public enum RsaFormat
        {
            SHA1,
            SHA256
        }

        #region RSA私有方法
        private static int GetIntegerSize(BinaryReader binr)
        {
            byte bt = 0;
            byte lowbyte = 0x00;
            byte highbyte = 0x00;
            int count = 0;
            bt = binr.ReadByte();
            if (bt != 0x02)
            {
                return 0;
            }

            bt = binr.ReadByte();

            if (bt == 0x81)
            {
                count = binr.ReadByte();
            }
            else if (bt == 0x82)
            {
                highbyte = binr.ReadByte();
                lowbyte = binr.ReadByte();
                byte[] modint = { lowbyte, highbyte, 0x00, 0x00 };
                count = BitConverter.ToInt32(modint, 0);
            }
            else
            {
                count = bt;
            }

            while (binr.ReadByte() == 0x00)
            {
                count -= 1;
            }
            binr.BaseStream.Seek(-1, SeekOrigin.Current);
            return count;
        }

        private static RSA DecodeRsaPrivateKey(byte[] privkey, RsaFormat signType)
        {
            byte[] MODULUS, E, D, P, Q, DP, DQ, IQ;
            var mem = new MemoryStream(privkey);
            var binr = new BinaryReader(mem);
            byte bt = 0;
            ushort twobytes = 0;
            int elems = 0;
            try
            {
                twobytes = binr.ReadUInt16();
                if (twobytes == 0x8130)
                {
                    binr.ReadByte();
                }
                else if (twobytes == 0x8230)
                {
                    binr.ReadInt16();
                }
                else
                {
                    return null;
                }

                twobytes = binr.ReadUInt16();
                if (twobytes != 0x0102)
                {
                    return null;
                }

                bt = binr.ReadByte();
                if (bt != 0x00)
                {
                    return null;
                }

                elems = GetIntegerSize(binr);
                MODULUS = binr.ReadBytes(elems);

                elems = GetIntegerSize(binr);
                E = binr.ReadBytes(elems);

                elems = GetIntegerSize(binr);
                D = binr.ReadBytes(elems);

                elems = GetIntegerSize(binr);
                P = binr.ReadBytes(elems);

                elems = GetIntegerSize(binr);
                Q = binr.ReadBytes(elems);

                elems = GetIntegerSize(binr);
                DP = binr.ReadBytes(elems);

                elems = GetIntegerSize(binr);
                DQ = binr.ReadBytes(elems);

                elems = GetIntegerSize(binr);
                IQ = binr.ReadBytes(elems);

                var bitLen = 1024;
                if (signType == RsaFormat.SHA256)
                {
                    bitLen = 2048;
                }

                var rsa = RSA.Create();
                rsa.KeySize = bitLen;
                var rsAparams = new RSAParameters
                {
                    Modulus = MODULUS,
                    Exponent = E,
                    D = D,
                    P = P,
                    Q = Q,
                    DP = DP,
                    DQ = DQ,
                    InverseQ = IQ
                };
                rsa.ImportParameters(rsAparams);
                return rsa;
            }
            catch (Exception)
            {
                return null;
            }
            finally
            {
                binr.Close();
                mem.Dispose();
            }
        }

        private static bool CompareBytearrays(byte[] a, byte[] b)
        {
            if (a.Length != b.Length)
            {
                return false;
            }
            int i = 0;
            foreach (byte c in a)
            {
                if (c != b[i])
                {
                    return false;
                }
                i++;
            }
            return true;
        }

        private static RSA CreateRsaProviderFromPublicKey(string publicKeyString, RsaFormat signType)
        {
            byte[] seqOid = { 0x30, 0x0D, 0x06, 0x09, 0x2A, 0x86, 0x48, 0x86, 0xF7, 0x0D, 0x01, 0x01, 0x01, 0x05, 0x00 };
            byte[] seq = new byte[15];

            var x509Key = Convert.FromBase64String(publicKeyString);
            using (var mem = new MemoryStream(x509Key))
            {
                using (var binr = new BinaryReader(mem))
                {
                    byte bt = 0;
                    ushort twobytes = 0;

                    twobytes = binr.ReadUInt16();
                    if (twobytes == 0x8130)
                    {
                        binr.ReadByte();
                    }
                    else if (twobytes == 0x8230)
                    {
                        binr.ReadInt16();
                    }
                    else
                    {
                        return null;
                    }

                    seq = binr.ReadBytes(15);
                    if (!CompareBytearrays(seq, seqOid))
                    {
                        return null;
                    }

                    twobytes = binr.ReadUInt16();
                    if (twobytes == 0x8103)
                    {
                        binr.ReadByte();
                    }
                    else if (twobytes == 0x8203)
                    {
                        binr.ReadInt16();
                    }
                    else
                    {
                        return null;
                    }

                    bt = binr.ReadByte();
                    if (bt != 0x00)
                    {
                        return null;
                    }

                    twobytes = binr.ReadUInt16();
                    if (twobytes == 0x8130)
                    {
                        binr.ReadByte();
                    }
                    else if (twobytes == 0x8230)
                    {
                        binr.ReadInt16();
                    }
                    else
                    {
                        return null;
                    }

                    twobytes = binr.ReadUInt16();
                    byte lowbyte = 0x00;
                    byte highbyte = 0x00;

                    if (twobytes == 0x8102)
                    {
                        lowbyte = binr.ReadByte();
                    }
                    else if (twobytes == 0x8202)
                    {
                        highbyte = binr.ReadByte();
                        lowbyte = binr.ReadByte();
                    }
                    else
                    {
                        return null;
                    }
                    byte[] modint = { lowbyte, highbyte, 0x00, 0x00 };
                    int modsize = BitConverter.ToInt32(modint, 0);

                    int firstbyte = binr.PeekChar();
                    if (firstbyte == 0x00)
                    {
                        binr.ReadByte();
                        modsize -= 1;
                    }

                    byte[] modulus = binr.ReadBytes(modsize);

                    if (binr.ReadByte() != 0x02)
                    {
                        return null;
                    }
                    int expbytes = binr.ReadByte();
                    byte[] exponent = binr.ReadBytes(expbytes);

                    var rsa = RSA.Create();
                    rsa.KeySize = signType == RsaFormat.SHA1 ? 1024 : 2048;
                    var rsaKeyInfo = new RSAParameters
                    {
                        Modulus = modulus,
                        Exponent = exponent
                    };
                    rsa.ImportParameters(rsaKeyInfo);

                    return rsa;
                }
            }
        }
        #endregion

        /// <summary> 使用 RSA 公钥加密 </summary>
        public static string RsaEncrypt(string message, string publicKey)
        {
            using (var rsa = new RSACryptoServiceProvider())
            {
                rsa.FromXmlString(publicKey);

                var messageBytes = Encoding.UTF8.GetBytes(message);

                var resultBytes = rsa.Encrypt(messageBytes, false);

                return Convert.ToBase64String(resultBytes);
            }
        }

        /// <summary> 使用 RSA 私钥解密 </summary>
        public static string RsaDecrypt(string message, string privateKey)
        {
            using (var rsa = new RSACryptoServiceProvider())
            {
                rsa.FromXmlString(privateKey);

                var messageBytes = Convert.FromBase64String(message);

                var resultBytes = rsa.Decrypt(messageBytes, false);

                return Encoding.UTF8.GetString(resultBytes);
            }
        }

        /// <summary> 使用 RSA 私钥签名 </summary>
        public static string RsaSignature(string message, string privateKey, string charset = "utf-8",
            RsaFormat signType = RsaFormat.SHA1)
        {
            byte[] signatureBytes = null;
            try
            {
                var data = Convert.FromBase64String(privateKey);
                var rsa = DecodeRsaPrivateKey(data, signType);
                using (rsa)
                {
                    var messageBytes = Encoding.GetEncoding(charset).GetBytes(message);
                    var hashName = signType == RsaFormat.SHA1 ? HashAlgorithmName.SHA1 : HashAlgorithmName.SHA256;
                    signatureBytes = rsa.SignData(messageBytes, hashName, RSASignaturePadding.Pkcs1);
                }
            }
            catch (Exception)
            {
                throw new Exception($"您使用的私钥格式错误，请检查RSA私钥配置,charset = {charset}");
            }

            return Convert.ToBase64String(signatureBytes);
        }

        /// <summary> 使用 RSA 私钥签名 </summary>
        [Obsolete]
        public static string RsaSignature(string message, string privateKey, string charset = "utf-8", string signType = "RSA")
        {
            return RsaSignature(message, privateKey, charset, signType == "RSA" ? RsaFormat.SHA1 : RsaFormat.SHA256);
        }

        /// <summary>
        /// 使用 RSA 公钥验证签名
        /// </summary>
        public static bool RsaVerifySign(string message, string signature, string publicKey, string charset = "utf-8",
            RsaFormat signType = RsaFormat.SHA1)
        {
            try
            {
                var sPublicKeyPem = publicKey;
                var rsa = CreateRsaProviderFromPublicKey(sPublicKeyPem, signType);

                var hashName = signType == RsaFormat.SHA1 ? HashAlgorithmName.SHA1 : HashAlgorithmName.SHA256;
                var data = Encoding.GetEncoding(charset).GetBytes(message);
                return rsa.VerifyData(data, Convert.FromBase64String(signature), hashName, RSASignaturePadding.Pkcs1);
            }
            catch
            {
                return false;
            }
        }

        /// <summary> 使用 RSA 公钥验证签名 </summary>
        [Obsolete]
        public static bool RsaVerifySign(string message, string signature, string publicKey, string charset = "utf-8",
            string signType = "RSA")
        {
            return RsaVerifySign(message, signature, publicKey, charset,
                signType == "RSA" ? RsaFormat.SHA1 : RsaFormat.SHA256);
        }

        /// <summary>
        /// MD5加密
        /// </summary>
        /// <param name="data">数据</param>
        public static string MD5(string data)
        {
            return MD5(data, Encoding.UTF8);
        }

        /// <summary> MD5加密 </summary>
        /// <param name="data">数据</param>
        /// <param name="encoding">编码</param>
        /// <returns></returns>
        public static string MD5(string data, Encoding encoding)
        {
            var md5 = System.Security.Cryptography.MD5.Create();
            var dataByte = md5.ComputeHash(encoding.GetBytes(data));
            var sb = new StringBuilder();
            foreach (var b in dataByte)
            {
                sb.Append(b.ToString("X2"));
            }

            return sb.ToString();
        }
    }
}