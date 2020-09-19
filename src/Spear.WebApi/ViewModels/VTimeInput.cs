using Spear.Core.Domain.Dtos;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Spear.WebApi.ViewModels
{
    public class VTimeInput : ITimeInput, IValidatableObject
    {
        /// <inheritdoc />
        /// <summary> 开始时间（大于等于） </summary>   
        public DateTime? Begin { get; set; }

        /// <inheritdoc />
        /// <summary> 截止时间（小于） </summary>
        public DateTime? End { get; set; }

        public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var list = new List<ValidationResult>();
            if (Begin.HasValue && End.HasValue && Begin.Value >= End.Value)
                list.Add(new ValidationResult("开始时间需要大于结束时间"));
            return list;
        }
    }
}
