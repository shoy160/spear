using BenchmarkDotNet.Attributes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace Spear.Tests.Client.Benchmark
{
    //[RankColumn]
    public class DeserializeBenchmarks
    {
        private static readonly string LargeJsonText;
        private static readonly string FloatArrayJson;

        static DeserializeBenchmarks()
        {
            LargeJsonText = System.IO.File.ReadAllText("large.json".ResolvePath());

            FloatArrayJson = new JArray(Enumerable.Range(0, 5000).Select(i => i * 1.1m)).ToString(Formatting.None);
        }

        [Benchmark]
        public IList<RootObject> DeserializeLargeJsonText()
        {
            return JsonConvert.DeserializeObject<IList<RootObject>>(LargeJsonText);
        }

        [Benchmark]
        public IList<RootObject> DeserializeLargeJsonFile()
        {
            using (var jsonFile = System.IO.File.OpenText("large.json".ResolvePath()))
            using (JsonTextReader jsonTextReader = new JsonTextReader(jsonFile))
            {
                JsonSerializer serializer = new JsonSerializer();
                return serializer.Deserialize<IList<RootObject>>(jsonTextReader);
            }
        }

        [Benchmark]
        public IList<double> DeserializeDoubleList()
        {
            return JsonConvert.DeserializeObject<IList<double>>(FloatArrayJson);
        }

        [Benchmark]
        public IList<decimal> DeserializeDecimalList()
        {
            return JsonConvert.DeserializeObject<IList<decimal>>(FloatArrayJson);
        }
    }
}
