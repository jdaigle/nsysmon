using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace NSysmon.Core.PerfMon
{
    public class CounterDictionary
    {
        public CounterDictionary()
        {
            this.Counters = new Dictionary<string, CounterDictionaryKey>();
        }

        [JsonIgnore]
        public string Path { get; private set; }

        [JsonProperty]
        private Dictionary<string, CounterDictionaryKey> Counters { get; set; }

        public Guid GetCounterIdOrNew(string hostname, string category, string name, string instance)
        {
            var _key = (hostname + "_" + category + "_" + name + "_" + instance).ToUpperInvariant();
            if (!Counters.ContainsKey(_key))
            {
                Counters.Add(_key, new CounterDictionaryKey(hostname, category, name, instance));
                WriteToFile(this.Path, this);
            }
            return Counters[_key].CounterId;
        }

        public static CounterDictionary ReadFromFile(string path)
        {
            CounterDictionary dictionary = null;
            if (!File.Exists(path))
            {
                dictionary = new CounterDictionary();
            }
            else
            {
                dictionary = JsonConvert.DeserializeObject<CounterDictionary>(File.ReadAllText(path));
            }
            dictionary.Path = path;
            return dictionary;
        }

        public static void WriteToFile(string path, CounterDictionary dictionary)
        {
            File.WriteAllText(path, JsonConvert.SerializeObject(dictionary, Formatting.Indented));
        }
    }
}
