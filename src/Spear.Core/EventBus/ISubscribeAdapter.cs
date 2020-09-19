using Spear.Core.Dependency;

namespace Spear.Core.EventBus
{
    /// <summary> 订阅适配器 </summary>
    public interface ISubscribeAdapter : ISingleDependency
    {
        /// <summary> 订阅 </summary>
        void SubscribeAt();
    }
}
