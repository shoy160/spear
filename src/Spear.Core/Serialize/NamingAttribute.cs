using System;

namespace Spear.Core.Serialize
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = false)]
    public class NamingAttribute : Attribute
    {
        public string Name { get; set; }
        public bool Ignore { get; set; }

        public NamingType NamingType { get; set; }

        public NamingAttribute(NamingType namingType)
        {
            NamingType = namingType;
        }

        public NamingAttribute(string name)
        {
            Name = name;
        }
        public NamingAttribute(bool ignore)
        {
            Ignore = ignore;
        }
    }
}
