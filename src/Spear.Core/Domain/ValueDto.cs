using System;
using Spear.Core.Domain.Dtos;

namespace Spear.Core.Domain
{
    /// <summary> key - value </summary>
    /// <typeparam name="T"></typeparam>
    public class ValueDto<T> : ValueDto<string, T> { }

    /// <summary> key - value </summary>
    public class ValueDto : ValueDto<string, string> { }

    /// <summary> key - value </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    [Serializable]
    public class ValueDto<TKey, TValue> : DDto
    {
        /// <summary> Key </summary>
        public TKey Key { get; set; }
        /// <summary> Value </summary>
        public TValue Value { get; set; }

        public ValueDto() { }

        public ValueDto(TKey key, TValue value)
        {
            Key = key;
            Value = value;
        }
    }

    /// <summary> id - name </summary>
    public class NameDto<T> : NameDto<string, T> { }

    /// <summary> id - name </summary>
    public class NameDto : NameDto<string, string> { }

    /// <summary> id - name </summary>
    /// <typeparam name="TId"></typeparam>
    /// <typeparam name="TName"></typeparam>
    [Serializable]
    public class NameDto<TId, TName> : DDto
    {
        /// <summary> id </summary>
        public TId Id { get; set; }

        /// <summary> name </summary>
        public TName Name { get; set; }

        public NameDto() { }
        public NameDto(TId id, TName name)
        { Id = id; Name = name; }

    }

    /// <summary> key - count </summary>
    public class CountDto<T> : CountDto<string, T> where T : struct { }

    /// <summary> key - count </summary>
    public class CountDto : CountDto<string, int> { }

    /// <summary> key - count </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TCount"></typeparam>
    [Serializable]
    public class CountDto<TKey, TCount> : DDto
        where TCount : struct
    {
        /// <summary> key </summary>
        public TKey Key { get; set; }
        /// <summary> count </summary>
        public TCount Count { get; set; }

        public CountDto() { }
        public CountDto(TKey key, TCount count)
        {
            Key = key;
            Count = count;
        }
    }
}
