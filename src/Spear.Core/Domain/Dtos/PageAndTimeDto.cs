namespace Spear.Core.Domain.Dtos
{
    public class PageAndTimeDto : TimeInputDto, IPageInput
    {
        /// <inheritdoc />
        /// <summary> 当前页码 </summary>
        public int Page { get; set; }

        /// <inheritdoc />
        /// <summary> 每页数量 </summary>
        public int Size { get; set; }

        /// <summary> 构造函数 </summary>
        public PageAndTimeDto()
        {
            Page = 1;
            Size = 20;
        }

        /// <summary> 构造函数 </summary>
        /// <param name="page"></param>
        /// <param name="size"></param>
        public PageAndTimeDto(int page, int size)
        {
            Page = page;
            Size = size;
        }
    }
}
