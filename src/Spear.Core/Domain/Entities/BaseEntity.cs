using System.Collections.Generic;

namespace Spear.Core.Domain.Entities
{
    /// <summary> 数据库基础实体 </summary>
    public abstract class BaseEntity : BaseEntity<int> { }

    /// <summary> 数据库基础实体 </summary>
    public abstract class BaseEntity<TKey> : IEntity<TKey>
    {
        public virtual TKey Id { get; set; }

        /// <summary> 是否主键ID有值 </summary>
        /// <returns></returns>
        public virtual bool IsTransient()
        {
            return EqualityComparer<TKey>.Default.Equals(Id, default(TKey));
        }

        /// <summary>
        /// 判断两个实体是否是同一数据记录的实体
        /// </summary>
        /// <param name="obj">要比较的实体信息</param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (!(obj is BaseEntity<TKey>))
            {
                return false;
            }

            //Same instances must be considered as equal
            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            //Transient objects are not considered as equal
            var other = (BaseEntity<TKey>)obj;
            if (IsTransient() && other.IsTransient())
            {
                return false;
            }
            var typeOfThis = GetType();
            var typeOfOther = other.GetType();
            if (!typeOfThis.IsAssignableFrom(typeOfOther) && !typeOfOther.IsAssignableFrom(typeOfThis))
            {
                return false;
            }
            return Id.Equals(other.Id);
        }

        public static bool operator ==(BaseEntity<TKey> left, BaseEntity<TKey> right)
        {
            return (Equals(left, null) ? Equals(right, null) : left.Equals(right));
        }

        public static bool operator !=(BaseEntity<TKey> left, BaseEntity<TKey> right)
        {
            return !(left == right);
        }

        /// <summary>
        /// 用作特定类型的哈希函数。
        /// </summary>
        /// <returns>
        /// 当前 <see cref="T:System.Object"/> 的哈希代码。
        /// </returns>
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"[{GetType().Name} {Id}]";
        }
    }
}
