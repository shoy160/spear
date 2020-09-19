using Spear.Core.Domain.Dtos;
using System.ComponentModel.DataAnnotations;

namespace Spear.WebApi.ViewModels
{
    public class VPageAndTimeInput : VTimeInput, IPageInput
    {
        /// <inheritdoc />
        /// <summary> 当前页码 </summary>
        [Range(1, 2000, ErrorMessage = "当前页面必须在1-2000之间")]
        public int Page { get; set; }

        /// <inheritdoc />
        /// <summary> 每页数量 </summary>
        [Range(1, 500, ErrorMessage = "每页数量必须在1-500之间")]
        public int Size { get; set; }
    }
}
