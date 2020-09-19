using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Spear.Core.Extensions
{
    /// <summary> 表达式扩展 </summary>
    public static class ExpressionExtension
    {
        /// <summary>
        /// 以特定的条件运行组合两个Expression表达式
        /// </summary>
        /// <typeparam name="T">表达式的主实体类型</typeparam>
        /// <param name="first">第一个Expression表达式</param>
        /// <param name="second">要组合的Expression表达式</param>
        /// <param name="merge">组合条件运算方式</param>
        /// <returns>组合后的表达式</returns>
        public static Expression<T> Compose<T>(this Expression<T> first, Expression<T> second,
            Func<Expression, Expression, Expression> merge)
        {
            Dictionary<ParameterExpression, ParameterExpression> map =
                first.Parameters.Select((f, i) => new { f, s = second.Parameters[i] }).ToDictionary(p => p.s, p => p.f);
            Expression secondBody = ParameterRebinder.ReplaceParameters(map, second.Body);
            return Expression.Lambda<T>(merge(first.Body, secondBody), first.Parameters);
        }

        /// <summary>
        /// 以 Expression.AndAlso 组合两个Expression表达式
        /// </summary>
        /// <typeparam name="T">表达式的主实体类型</typeparam>
        /// <param name="first">第一个Expression表达式</param>
        /// <param name="second">要组合的Expression表达式</param>
        /// <returns>组合后的表达式</returns>
        public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> first,
            Expression<Func<T, bool>> second)
        {
            return first.Compose(second, Expression.AndAlso);
        }

        /// <summary>
        /// 以 Expression.OrElse 组合两个Expression表达式
        /// </summary>
        /// <typeparam name="T">表达式的主实体类型</typeparam>
        /// <param name="first">第一个Expression表达式</param>
        /// <param name="second">要组合的Expression表达式</param>
        /// <returns>组合后的表达式</returns>
        public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> first,
            Expression<Func<T, bool>> second)
        {
            return first.Compose(second, Expression.OrElse);
        }

        /// <summary> 表达式成员名 </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="func"></param>
        /// <returns></returns>
        public static IEnumerable<string> MemberNames<T>(this Expression<T> func)
        {
            return func.Body.MemberNames();
        }

        /// <summary> 表达式成员名称 </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public static IEnumerable<string> MemberNames(this Expression expression)
        {
            var type = expression.GetType().GetTypeInfo();
            if (type.IsGenericType)
                expression = type.GetProperty("Body")?.GetValue(expression) as Expression;
            if (expression is NewExpression newExpression)
                return newExpression.Members.Select(t => t.Name);

            return new List<string>();
        }

        private class ParameterRebinder : ExpressionVisitor
        {
            private readonly Dictionary<ParameterExpression, ParameterExpression> _map;

            private ParameterRebinder(Dictionary<ParameterExpression, ParameterExpression> map)
            {
                _map = map ?? new Dictionary<ParameterExpression, ParameterExpression>();
            }

            public static Expression ReplaceParameters(Dictionary<ParameterExpression, ParameterExpression> map,
                Expression exp)
            {
                return new ParameterRebinder(map).Visit(exp);
            }

            protected override Expression VisitParameter(ParameterExpression node)
            {
                if (_map.TryGetValue(node, out var replacement))
                {
                    node = replacement;
                }
                return base.VisitParameter(node);
            }
        }
    }
}