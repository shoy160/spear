using System;
using System.Threading;

namespace Spear.Core
{
    public class DisposeAction : IDisposable
    {
        private Action _action;

        public DisposeAction(Action action)
        {
            _action = action;
        }


        public void Dispose()
        {
            var action = Interlocked.Exchange(ref _action, null);
            action?.Invoke();
        }
        public static readonly DisposeAction Empty = new DisposeAction(null);
    }
}
