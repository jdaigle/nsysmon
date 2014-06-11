using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NSysmon.Forwarder
{
    public class CounterTimestamp
    {
        public CounterTimestamp() { }
        public CounterTimestamp(DateTime timestamp, float value)
        {
            this.timestamp = timestamp.ToString("o");
            this.value = value;
        }

        public string timestamp { get; set; }
        public float value { get; set; }
    }
}
