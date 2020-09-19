using Spear.Core.Dependency;
using System.Threading.Tasks;

namespace Spear.Core.EventBus
{
    /// <summary> 事件处理 </summary>
    public interface IEventHandler : ISingleDependency
    {
    }

    /// <summary> 事件处理 </summary>
    /// <typeparam name="TEvent"></typeparam>
    public interface IEventHandler<in TEvent> : IEventHandler
    {
        /// <summary>
        /// 事件处理
        /// 业务异常将不会重试，其他异常会自动重试5次
        /// 前3次间隔为10^N秒,第4次12小时,第5次24小时
        /// </summary>
        /// <param name="event"></param>
        /// <returns></returns>
        Task Handle(TEvent @event);
    }
}
