using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSysmon.Collector.PerfMon
{
    public sealed class CounterDictionaryKey
    {
        public CounterDictionaryKey() { }
        public CounterDictionaryKey(string hostname, string cat, string name, string instance)
        {
            this.CounterId = Guid.NewGuid();
            this.Hostname = hostname;
            this.PerformanceCounterCategory = cat;
            this.PerformanceCounterName = name;
            this.PerformanceCounterInstance = instance;
        }

        public Guid CounterId { get; set; }
        public string Hostname { get; set; }
        public string PerformanceCounterCategory { get; set; }
        public string PerformanceCounterName { get; set; }
        public string PerformanceCounterInstance { get; set; }
    }
}
