using System;

namespace Spear.Core.Domain.Dtos
{
    public interface ITimeInput
    {
        /// <summary> 开始时间（大于等于） </summary>        
        DateTime? Begin { get; set; }

        /// <summary> 截止时间（小于） </summary>
        DateTime? End { get; set; }
    }

    public class TimeInputDto : DDto, ITimeInput
    {
        /// <summary> 开始时间（大于等于） </summary>        
        public DateTime? Begin { get; set; }

        /// <summary> 截止时间（小于） </summary>
        public DateTime? End { get; set; }
    }
}
