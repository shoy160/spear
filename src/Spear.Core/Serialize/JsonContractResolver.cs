using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Spear.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Spear.Core.Serialize
{
    internal class JsonContractResolver : DefaultContractResolver
    {
        private readonly IDictionary<string, string> _replaceProps;
        private readonly string[] _props;
        private readonly bool _retain;
        private readonly NamingType _camelCase;

        public JsonContractResolver(NamingType camelCase, IDictionary<string, string> replaceProps = null)
        {
            _camelCase = camelCase;
            _replaceProps = replaceProps;
        }

        /// <summary> 构造函数 </summary>
        /// <param name="camelCase">驼峰</param>
        /// <param name="retain">保留/排除：true为保留</param>
        /// <param name="replaceProps">需替换的属性</param>
        /// <param name="props"></param>
        public JsonContractResolver(NamingType camelCase = NamingType.Normal, bool retain = true,
            IDictionary<string, string> replaceProps = null, params string[] props)
        {
            _replaceProps = replaceProps;
            _camelCase = camelCase;
            _retain = retain;
            _props = props;
        }

        protected override string ResolvePropertyName(string propertyName)
        {
            if (_replaceProps != null && _replaceProps.TryGetValue(propertyName, out var newName))
                propertyName = newName;
            switch (_camelCase)
            {
                case NamingType.CamelCase:
                    return propertyName.ToCamelCase();
                case NamingType.UrlCase:
                    return propertyName.ToUrlCase();
                default:
                    return base.ResolvePropertyName(propertyName);
            }
        }

        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            var propList = base.CreateProperties(type, memberSerialization);
            if (_props == null || _props.Length == 0)
                return propList;
            return
                propList.Where(
                    p => _retain
                        ? _props.Contains(p.PropertyName)
                        : !_props.Contains(p.PropertyName))
                    .ToList();
        }
    }
}
