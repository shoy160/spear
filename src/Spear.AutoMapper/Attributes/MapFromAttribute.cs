using System;

namespace Spear.AutoMapper.Attributes
{
    /// <summary> Map 字段映射 </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class MapFromAttribute : Attribute
    {
        /// <summary>  映射字段  </summary>
        public string Name { get; private set; }


        public MapFromAttribute(string name)
        {
            Name = name;
        }
    }
}
