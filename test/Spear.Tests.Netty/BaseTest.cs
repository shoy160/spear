using DotNetty.Buffers;
using System;
using System.Threading.Tasks;

namespace Spear.Tests.Netty
{
    public abstract class BaseTest : IDisposable
    {
        public virtual Task OnCommand(string cmd) { return Task.CompletedTask; }

        protected IByteBuffer Encode(string msg)
        {
            return msg.Encode();
        }

        protected string Decode(object msg)
        {
            return msg.Decode();
        }

        public virtual void Dispose()
        {
        }
    }
}
