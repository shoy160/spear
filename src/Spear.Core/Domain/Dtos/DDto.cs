using Spear.Core.Serialize;
using System;

namespace Spear.Core.Domain.Dtos
{
    public interface IDto { }

    /// <summary>
    /// 数据传输实体基类
    /// </summary>
    [Serializable]
    public abstract class DDto : IDto
    {
        public override string ToString()
        {
            //debug时缩进
#if DEBUG
            return JsonHelper.ToJson(this, NamingType.CamelCase, true);
#else
            return JsonHelper.ToJson(this, NamingType.CamelCase);
#endif
        }
    }
}
