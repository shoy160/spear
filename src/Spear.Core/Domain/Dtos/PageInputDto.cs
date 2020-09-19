namespace Spear.Core.Domain.Dtos
{
    /// <summary> 分页基础接口 </summary>
    public interface IPageInput
    {
        /// <summary> 当前页码 </summary>
        int Page { get; set; }

        /// <summary> 每页数量 </summary>
        int Size { get; set; }
    }

    /// <summary> 分页实体 </summary>
    public class PageInputDto : DDto, IPageInput
    {
        /// <summary> 当前页码 </summary>
        public int Page { get; set; }

        /// <summary> 每页数量 </summary>
        public int Size { get; set; }

        /// <summary> 构造函数 </summary>
        public PageInputDto()
        {
            Page = 1;
            Size = 20;
        }

        /// <summary> 构造函数 </summary>
        /// <param name="page"></param>
        /// <param name="size"></param>
        public PageInputDto(int page, int size)
        {
            Page = page;
            Size = size;
        }
    }
}
