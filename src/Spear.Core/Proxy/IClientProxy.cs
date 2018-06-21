using Spear.Core.Micro;

namespace Spear.Core.Proxy
{
    public interface IClientProxy
    {
        void SetClient(IMicroClientFactory clientFactory);
        T Create<T>();
    }
}
