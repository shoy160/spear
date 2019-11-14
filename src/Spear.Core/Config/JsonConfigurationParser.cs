using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Spear.Core.Config
{
    public class JsonConfigurationParser
    {
        private readonly IDictionary<string, string> _data;
        private readonly Stack<string> _context;
        private string _currentPath;
        private JsonTextReader _reader;

        public JsonConfigurationParser()
        {
            _data = new SortedDictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            _context = new Stack<string>();
        }

        /// <summary> 转换Json字符 </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public IDictionary<string, string> Parse(string json)
        {
            _data.Clear();
            _reader =
                new JsonTextReader(new StringReader(json)) { DateParseHandling = DateParseHandling.None };
            VisitJObject(JObject.Load(_reader));
            return _data;
        }

        /// <summary> 转换json流 </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public IDictionary<string, string> Parse(Stream input)
        {
            _data.Clear();
            _reader =
                new JsonTextReader(new StreamReader(input)) { DateParseHandling = DateParseHandling.None };
            VisitJObject(JObject.Load(_reader));
            return _data;
        }

        private void VisitJObject(JObject jObject)
        {
            foreach (var property in jObject.Properties())
            {
                EnterContext(property.Name);
                VisitProperty(property);
                ExitContext();
            }
        }

        private void VisitProperty(JProperty property)
        {
            VisitToken(property.Value);
        }

        private void VisitToken(JToken token)
        {
            switch (token.Type)
            {
                case JTokenType.Object:
                    VisitJObject(token.Value<JObject>());
                    break;
                case JTokenType.Array:
                    VisitArray(token.Value<JArray>());
                    break;
                case JTokenType.Integer:
                case JTokenType.Float:
                case JTokenType.String:
                case JTokenType.Boolean:
                case JTokenType.Null:
                case JTokenType.Raw:
                case JTokenType.Bytes:
                    VisitPrimitive(token.Value<JValue>());
                    break;
                default:
                    throw new FormatException(
                        $"{_reader.TokenType}, {_reader.Path}, {_reader.LineNumber}, {_reader.LinePosition}");
            }
        }

        private void VisitArray(JArray array)
        {
            for (var index = 0; index < array.Count; ++index)
            {
                EnterContext(index.ToString());
                VisitToken(array[index]);
                ExitContext();
            }
        }

        private void VisitPrimitive(JValue data)
        {
            string currentPath = _currentPath;
            if (_data.ContainsKey(currentPath))
                throw new FormatException(currentPath);
            _data[currentPath] = data.ToString(CultureInfo.InvariantCulture);
        }

        private void EnterContext(string context)
        {
            _context.Push(context);
            _currentPath = ConfigurationPath.Combine(_context.Reverse());
        }

        private void ExitContext()
        {
            _context.Pop();
            _currentPath = ConfigurationPath.Combine(_context.Reverse());
        }
    }
}
