using Spear.Core.Domain.Dtos;
using System.ComponentModel.DataAnnotations;

namespace Spear.WebApi.ViewModels
{
    public class VPageInput : IPageInput
    {
        /// <inheritdoc />
        /// <summary> 当前页码 </summary>
        [Range(1, int.MaxValue, ErrorMessage = "当前页面必须大于1")]
        public int Page { get; set; } = 1;

        /// <inheritdoc />
        /// <summary> 每页数量 </summary>
        [Range(1, 5000, ErrorMessage = "每页数量必须在1-500之间")]
        public int Size { get; set; } = 10;
    }
}
