namespace Spear.WebApi
{
    /// <inheritdoc />
    /// <summary> 基础身份验证 </summary>
    /// <typeparam name="TTicket"></typeparam>
    public class DAuthorController<TTicket> : DController where TTicket : IClientTicket
    {
        /// <summary> 当前用户 </summary>
        public virtual TTicket Client => ControllerContext.HttpContext.Request.VerifyTicket<TTicket>();
    }
}
