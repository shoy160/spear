using DotNetty.Buffers;
using System.Text;

namespace Spear.Tests.Netty
{
    public static class Extensions
    {
        public static IByteBuffer Encode(this string msg)
        {
            var buffer = Encoding.UTF8.GetBytes(msg);
            return Unpooled.WrappedBuffer(buffer);
        }

        public static string Decode(this object msg)
        {
            var buffer = (IByteBuffer)msg;
            var data = new byte[buffer.ReadableBytes];
            buffer.ReadBytes(data);
            return Encoding.UTF8.GetString(data);
        }
    }
}
