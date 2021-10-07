using Newtonsoft.Json;

namespace Spear.Sdk.Core.Helper
{
    public class JsonHelper
    {
        private static JsonSerializerSettings LoadSetting(bool indented = false)
        {
            var setting = new JsonSerializerSettings();
            if (indented)
                setting.Formatting = Formatting.Indented;
            setting.Converters.Add(new DateTimeConverter());
            setting.NullValueHandling = NullValueHandling.Ignore;
            return setting;
        }

        public static string ToJson(object obj, bool indented = false)
        {
            return obj == null ? string.Empty : JsonConvert.SerializeObject(obj, LoadSetting(indented));
        }

        public static T Json<T>(string json, T def = default)
        {
            try
            {
                return string.IsNullOrWhiteSpace(json) ? def : JsonConvert.DeserializeObject<T>(json, LoadSetting());
            }
            catch
            {
                return default;
            }
        }
    }
}
