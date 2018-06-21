using System.Threading;
using System.Threading.Tasks;

namespace Spear.Core.ServiceHosting
{
    public interface IHostLifetime
    {
        Task WaitForStart(CancellationToken cancellationToken);
        Task Stop(CancellationToken cancellationToken);
    }
}
