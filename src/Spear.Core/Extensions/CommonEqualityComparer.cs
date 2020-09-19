using System;
using System.Collections.Generic;

namespace Spear.Core.Extensions
{
    public class CommonEqualityComparer<T, V> : IEqualityComparer<T>
    {
        private readonly Func<T, V> _keySelector;
        private readonly IEqualityComparer<V> _comparer;

        public CommonEqualityComparer(Func<T, V> keySelector, IEqualityComparer<V> comparer)
        {
            _keySelector = keySelector;
            _comparer = comparer;
        }

        public CommonEqualityComparer(Func<T, V> keySelector)
            : this(keySelector, EqualityComparer<V>.Default)
        { }

        #region IEqualityComparer<T> 成员

        public bool Equals(T x, T y)
        {
            return _comparer.Equals(_keySelector(x), _keySelector(y));
        }

        public int GetHashCode(T obj)
        {
            return _comparer.GetHashCode(_keySelector(obj));
        }

        #endregion
    }
}
