using System.Threading;

namespace Spear.Core.ServiceHosting
{
    public interface IApplicationLifetime
    {
        CancellationToken Started { get; }

        CancellationToken Stopping { get; }

        CancellationToken Stopped { get; }


        void Stop();

        void NotifyStopped();

        void NotifyStarted();
    }
}
