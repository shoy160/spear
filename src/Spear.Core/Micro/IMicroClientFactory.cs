using System.Net;

namespace Spear.Core.Micro
{
    public interface IMicroClientFactory
    {
        /// <summary> 创建客户端 </summary>
        /// <param name="endPoint"></param>
        /// <returns></returns>
        IMicroClient CreateClient(EndPoint endPoint);
    }
}
