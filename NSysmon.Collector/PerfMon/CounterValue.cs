using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSysmon.Collector.PerfMon
{
    public sealed class CounterValue
    {
        public string Host { get; set; }
        public DateTime Timestamp { get; set; }
        public string Category { get; set; }
        public string Counter { get; set; }
        public string Instance { get; set; }
        public float Value { get; set; }
    }
}
