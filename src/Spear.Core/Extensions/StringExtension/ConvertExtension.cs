using Spear.Core.Helper;
using System;
using System.IO;

namespace Spear.Core.Extensions
{
    public static class ConvertExtension
    {
        public static int ToInt(this string c, int def)
        {
            return ConvertHelper.StrToInt(c, def);
        }

        public static int ToInt(this string c)
        {
            return c.ToInt(-1);
        }

        public static float ToFloat(this string c, float def)
        {
            return ConvertHelper.StrToFloat(c, def);
        }

        public static float ToFloat(this string c)
        {
            return c.ToFloat(-1F);
        }

        public static decimal ToDecimal(this string c, decimal def)
        {
            return (decimal)c.ToFloat((float)def);
        }

        public static decimal ToDecimal(this string c)
        {
            return (decimal)c.ToFloat(-1F);
        }

        public static DateTime ToDateTime(this string c, DateTime def)
        {
            return ConvertHelper.StrToDateTime(c, def);
        }

        public static DateTime ToDateTime(this string c)
        {
            return ConvertHelper.StrToDateTime(c);
        }

        public static byte[] ToBuffer(this string base64)
        {
            return Convert.FromBase64String(base64.CleanBase64());
        }

        public static Stream ToStream(this string base64)
        {
            var buffers = base64.CleanBase64().ToBuffer();
            return new MemoryStream(buffers);
        }
    }
}
