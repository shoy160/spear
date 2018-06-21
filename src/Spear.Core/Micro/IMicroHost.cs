using System.Net;
using System.Threading.Tasks;

namespace Spear.Core.Micro
{
    public interface IMicroHost
    {
        /// <summary> 启动微服务 </summary>
        /// <param name="endPoint"></param>
        /// <returns></returns>
        Task Start(EndPoint endPoint);
    }
}
