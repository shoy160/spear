namespace Spear.Core.Micro.Services
{
    public interface IServiceRegister
    {
        /// <summary> 注册服务 </summary>
        void Regist();

        /// <summary> 注销服务 </summary>
        void Deregist();
    }
}
