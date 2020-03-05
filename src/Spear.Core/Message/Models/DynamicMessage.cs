using System;
using Newtonsoft.Json;

namespace Spear.Core.Message.Models
{
    /// <summary> 动态实体 </summary>
    public class DynamicMessage
    {
        public string ContentType { get; set; }

        public object Content { get; set; }

        public DynamicMessage() { }

        public DynamicMessage(object data)
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
                ContentType = valueType.FullName;
            else
                ContentType = valueType.AssemblyQualifiedName;

            Content = JsonConvert.SerializeObject(data);
        }

        public object GetValue()
        {
            if (string.IsNullOrWhiteSpace(ContentType))
                return Content;
            var type = Type.GetType(ContentType);
            return JsonConvert.DeserializeObject(Content.ToString(), type);
        }
    }
}
