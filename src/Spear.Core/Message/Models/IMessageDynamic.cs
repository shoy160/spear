namespace Spear.Core.Message.Models
{
    /// <summary> 动态类型 </summary>
    public interface IMessageDynamic
    {
        /// <summary> 类型 </summary>
        string ContentType { get; set; }

        /// <summary> 数据 </summary>
        byte[] Content { get; set; }

        /// <summary> 设置对象 </summary>
        /// <param name="value"></param>
        void SetValue(object value);

        /// <summary> 获取对象 </summary>
        /// <returns></returns>
        object GetValue();
    }
}
