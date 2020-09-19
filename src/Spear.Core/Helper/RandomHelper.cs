using System;
using System.Linq;
using System.Text;

namespace Spear.Core.Helper
{
    /// <summary>
    /// 随机数辅助
    /// </summary>
    public static class RandomHelper
    {
        private const string AllLetter = "mnbvcxzlkjhgfdsapoiuytrewq";
        private const string HardWord = "0oOz29q1ilI6b";
        private const string AllWord = "qwNOPerWXYktyu421ioKfdsS867plVjMZ9hgDEnbTUxcABGHIJaCFmL0vzQR53";
        private const string SpecialWords = "!@#$%^&*_-";

        /// <summary>
        /// 获取线程级随机数
        /// </summary>
        /// <returns></returns>
        public static Random Random()
        {
            var bytes = new byte[4];
            var rng =
                new System.Security.Cryptography.RNGCryptoServiceProvider();
            rng.GetBytes(bytes);
            var seed = BitConverter.ToInt32(bytes, 0);
            var tick = DateTime.Now.Ticks + (seed);
            return new Random((int)(tick & 0xffffffffL) | (int)(tick >> 32));
        }

        /// <summary>
        /// 随机数字
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string RandomNums(int length)
        {
            if (length <= 0) return string.Empty;
            var sb = new StringBuilder();
            var random = Random();
            for (var i = 0; i < length; i++)
            {
                sb.Append(random.Next(0, 9));
            }
            return sb.ToString();
        }

        /// <summary>
        /// 随机字母
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string RandomLetters(int length)
        {
            if (length <= 0) return string.Empty;
            var sb = new StringBuilder();
            var random = Random();
            for (var i = 0; i < length; i++)
            {
                sb.Append(AllLetter[random.Next(0, AllLetter.Length - 1)]);
            }
            return sb.ToString();
        }

        /// <summary>
        /// 获取指定长度的随机字符串
        /// </summary>
        /// <param name="length">长度</param>
        /// <param name="hardWord">是否包含难辨别字符</param>
        /// <param name="hasSpecial">特殊字符</param>
        /// <returns></returns>
        public static string RandomNumAndLetters(int length, bool hardWord = false, bool hasSpecial = false)
        {
            if (length <= 0) return string.Empty;
            var list = AllWord.ToArray();
            if (!hardWord)
                list = list.Except(HardWord.ToArray()).ToArray();

            if (hasSpecial)
                list = list.Concat(SpecialWords.ToArray()).ToArray();
            var random = Random();
            var sb = new StringBuilder();
            for (var i = 0; i < length; i++)
            {
                sb.Append(list[random.Next(0, list.Length - 1)]);
            }
            return sb.ToString();
        }

        /// <summary>
        /// 随机汉字
        /// </summary>
        /// <param name="length">长度</param>
        /// <param name="level">汉字级别，1：1级，2：2级，3：1或2级，默认为1级</param>
        /// <returns></returns>
        public static string RandomHanzi(int length, int level = 1)
        {
            if (length <= 0) return string.Empty;
            var rm = Random();
            var bs = new byte[length * 2];
            var gb = Encoding.GetEncoding("gb2312");

            for (var i = 0; i < length; i++)
            {
                //16 - 55 区为一级汉字，按拼音排序
                //56 - 87 区为二级汉字，按部首/笔画排序
                //获取区码(常用汉字的区码范围为16-55)  
                int regionCode;
                switch (level)
                {
                    case 2:
                        regionCode = rm.Next(56, 88);
                        break;
                    case 3:
                        regionCode = rm.Next(16, 88);
                        break;
                    default:
                        regionCode = rm.Next(16, 56);
                        break;
                }
                // 获取位码(位码范围为1-94 由于55区的90,91,92,93,94为空,故将其排除)  
                // 55区排除90,91,92,93,94
                var positionCode = rm.Next(1, regionCode == 55 ? 90 : 95);
                // 转换区位码为机内码 + A0H
                bs[i * 2] = (byte)(regionCode + 0xa0);
                bs[i * 2 + 1] = (byte)(positionCode + 0xa0);
            }
            return gb.GetString(bs);
        }
    }
}
