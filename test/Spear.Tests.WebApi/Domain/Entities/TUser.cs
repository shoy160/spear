using Spear.Core.Domain.Entities;
using Spear.Core.Serialize;
using System;

namespace Spear.Tests.WebApi.Domain.Entities
{
    ///<summary> t_user </summary>
    [Naming("t_user", NamingType = NamingType.UrlCase)]
    public class TUser : BaseEntity<long>
    {
        ///<summary> id </summary>
        public override long Id { get; set; }

        ///<summary> 昵称 </summary>
        public string Nick { get; set; }

        ///<summary> 角色 </summary>
        public byte Role { get; set; }

        ///<summary> 头像 </summary>
        public string Avatar { get; set; }

        ///<summary> 创建时间 </summary>
        public DateTime CreateTime { get; set; }
    }

}
