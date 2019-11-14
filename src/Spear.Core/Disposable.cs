using System;
using System.Threading;

namespace Spear.Core
{
    /// <summary> 实现IDisposable接口，标示当前类型可释放 </summary>
    public abstract class Disposable : IDisposable
    {
        private bool _disposed;
        ~Disposable()
        {
            //析构函数调用时不释放托管资源，因为交由GC进行释放
            Dispose(false);
        }
        public void Dispose()
        {
            //用户手动释放托管资源和非托管资源
            Dispose(true);
            //用户已经释放了托管和非托管资源，所以不需要再调用析构函数
            GC.SuppressFinalize(this);
        }

        public virtual void Dispose(bool disposing)
        {
            if (_disposed) return;
            if (disposing)
            {
                //析构函数调用时不释放托管资源，因为交由GC进行释放
                //如果析构函数释放托管资源可能之前GC释放过，就会导致出现异常
                //托管资源释放
            }
            //非托管资源释放
            _disposed = true;
        }
    }
}
