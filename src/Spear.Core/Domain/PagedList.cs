using System;
using System.Collections.Generic;
using System.Linq;

namespace Spear.Core
{
    /// <summary> 数据分页 </summary>
    /// <typeparam name="TEntity"></typeparam>
    [Serializable]
    public class PagedList<TEntity>
    {
        /// <inheritdoc />
        public PagedList()
        {
            List = new List<TEntity>();
        }

        /// <summary> 构成函数 </summary>
        /// <param name="list"></param>
        /// <param name="total"></param>
        public PagedList(IEnumerable<TEntity> list, int total)
        {
            List = list;
            Total = total;
        }

        /// <summary> 构成函数 </summary>
        /// <param name="queryable"></param>
        /// <param name="index">页索引（从1开始）</param>
        /// <param name="size"></param>
        public PagedList(IQueryable<TEntity> queryable, int index, int size)
        {
            var total = queryable.Count();
            Total = total;
            if (Total > 0 && size > 0)
                Pages = (int)Math.Ceiling(Total / (float)size);

            Size = size;
            Index = index;
            List = queryable.Skip((index - 1) * size).Take(size).ToList();
        }

        /// <summary> 初始化数据 </summary>
        /// <param name="list"></param>
        /// <param name="index"></param>
        /// <param name="size"></param>
        /// <param name="total"></param>
        public PagedList(ICollection<TEntity> list, int index, int size, int total = 0)
        {
            Total = total <= 0 ? list.Count : total;
            if (Total > 0 && size > 0)
                Pages = (int)Math.Ceiling(Total / (float)size);

            Size = size;
            Index = index;
            List = list;
        }

        /// <summary> 页码(从1开始) </summary>
        public int Index { set; get; }
        /// <summary> 每页数量 </summary>
        public int Size { set; get; }
        /// <summary> 总数量 </summary>
        public int Total { set; get; }
        /// <summary> 总页数 </summary>
        public int Pages { set; get; }
        /// <summary> 是否有上一页 </summary>
        public bool HasPrev => Index > 1;
        /// <summary> 是否有下一页 </summary>
        public bool HasNext => Index < Pages;

        /// <summary> 列表数据 </summary>
        public IEnumerable<TEntity> List { get; set; }
    }
}
