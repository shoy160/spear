using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Spear.Sdk.Core.Helper
{
    internal class DateTimeConverter : DateTimeConverterBase
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteValue(0);
                return;
            }
            long timestamp = 0;
            if (value is DateTime dt)
            {
                timestamp = dt.Timestamp();
            }
            else if (value is DateTimeOffset offset)
            {
                dt = offset.DateTime;
                timestamp = dt.Timestamp();
            }
            //timestamp = timestamp < 0 ? 0 : timestamp;//时间小于1970会返回负数
            writer.WriteValue(timestamp);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (!(objectType == typeof(DateTime)) && !(objectType == typeof(DateTimeOffset)) &&
                (!(objectType == typeof(DateTime?)) && !(objectType == typeof(DateTimeOffset?))))
                throw new ArgumentException("时间格式异常", nameof(reader.Path));
            if (long.TryParse(reader.Value.ToString(), out var timestamp))
                return timestamp.Datetime();

            if (objectType.IsNullableType())
                return null;
            throw new ArgumentException("时间格式异常", nameof(reader.Path));
        }
    }
}
