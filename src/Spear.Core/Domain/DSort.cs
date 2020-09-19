using System;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;

namespace Spear.Core.Domain
{
    /// <summary>
    /// 列表字段排序条件
    /// </summary>
    public class DSort
    {
        /// <summary>
        /// 构造一个排序字段名称和排序方式的排序条件
        /// </summary>
        /// <param name="sortField">字段名称</param>
        /// <param name="listSortDirection">排序方式</param>
        public DSort(string sortField, ListSortDirection listSortDirection = ListSortDirection.Ascending)
        {
            SortField = sortField;
            ListSortDirection = listSortDirection;
        }

        /// <summary>
        /// 获取或设置 排序字段名称
        /// </summary>
        public string SortField { get; set; }

        /// <summary>
        /// 获取或设置 排序方向
        /// </summary>
        public ListSortDirection ListSortDirection { get; set; }
    }


    /// <summary>
    /// 支持泛型的列表字段排序条件
    /// </summary>
    /// <typeparam name="TEntity">列表元素类型</typeparam>
    /// <typeparam name="TProp"></typeparam>
    public class DSort<TEntity, TProp> : DSort
    {
        public Expression<Func<TEntity, TProp>> Selector { get; private set; }

        /// <summary>
        /// 使用排序字段与排序方式 初始化一个<see cref="Sort"/>类型的新实例
        /// </summary>
        public DSort(Expression<Func<TEntity, TProp>> keySelector,
            ListSortDirection listSortDirection = ListSortDirection.Ascending)
            : base(GetPropertyName(keySelector), listSortDirection)
        {
            Selector = keySelector;
        }

        /// <summary>
        /// 从泛型委托获取属性名
        /// </summary>
        private static string GetPropertyName(Expression<Func<TEntity, TProp>> keySelector)
        {
            var param = keySelector.Parameters.First().Name;
            string operand = (((dynamic)keySelector.Body).Operand).ToString();
            operand = operand.Substring(param.Length + 1, operand.Length - param.Length - 1);
            return operand;
        }
    }
}
