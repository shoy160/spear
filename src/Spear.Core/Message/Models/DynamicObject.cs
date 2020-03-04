using System;
using Newtonsoft.Json;

namespace Spear.Core.Message.Models
{
    /// <summary> 动态实体 </summary>
    public class DynamicObject
    {
        public string TypeName { get; set; }

        public object Content { get; set; }

        public object Data
        {
            get
            {
                if (string.IsNullOrWhiteSpace(TypeName))
                    return Content;
                var type = Type.GetType(TypeName);
                return JsonConvert.DeserializeObject(Content.ToString(), type);
            }
        }

        public DynamicObject() { }

        public DynamicObject(object data)
        {
            if (data == null)
                return;
            var valueType = data.GetType();
            var code = Type.GetTypeCode(valueType);
            if (code != TypeCode.Object)
            {
                Content = data;
                return;
            }

            if (code != TypeCode.Object && valueType.BaseType != typeof(Enum))
                TypeName = valueType.FullName;
            else
                TypeName = valueType.AssemblyQualifiedName;

            Content = JsonConvert.SerializeObject(data);
        }
    }
}
