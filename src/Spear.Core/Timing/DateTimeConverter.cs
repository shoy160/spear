using Spear.Core.Exceptions;
using Spear.Core.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

namespace Spear.Core.Timing
{
    public class DateTimeConverter : DateTimeConverterBase
    {
        /// <summary> 写操作 </summary>
        /// <param name="writer"></param>
        /// <param name="value"></param>
        /// <param name="serializer"></param>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteValue(0);
                return;
            }
            long timestamp = 0;
            if (value is DateTime)
            {
                var dt = (DateTime)value;
                timestamp = dt.ToMillisecondsTimestamp();
            }
            else if (value is DateTimeOffset)
            {
                var dt = ((DateTimeOffset)value).DateTime;
                timestamp = dt.ToMillisecondsTimestamp();
            }
            //timestamp = timestamp < 0 ? 0 : timestamp;//时间小于1970会返回负数
            writer.WriteValue(timestamp);
        }

        /// <summary> 读操作 </summary>
        /// <param name="reader"></param>
        /// <param name="objectType"></param>
        /// <param name="existingValue"></param>
        /// <param name="serializer"></param>
        /// <returns></returns>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (!(objectType == typeof(DateTime)) && !(objectType == typeof(DateTimeOffset)) &&
                (!(objectType == typeof(DateTime?)) && !(objectType == typeof(DateTimeOffset?))))
                throw new BusiException("时间格式异常");
            var timestamp = reader.Value.CastTo<long?>(null);//(long?)reader.Value ?? 0;
            if (timestamp.HasValue)
                return DateTimeHelper.FromMillisecondTimestamp(timestamp.Value);
            if (objectType.IsNullableType())
                return null;
            throw new BusiException("时间格式异常");

        }
    }
}
