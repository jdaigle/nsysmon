using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NSysmon.Core.Api
{
    public class CounterListViewModel : RazorViewModelBase
    {
        public CounterListViewModel()
        {
            this.Counters = new List<Counter>();
        }

        public List<Counter> Counters { get; set; }

        public class Counter
        {
            public Guid CounterId { get; set; }
            public string Hostname { get; set; }
            public string PerformanceCounterCategory { get; set; }
            public string PerformanceCounterName { get; set; }
            public string PerformanceCounterInstance { get; set; }

            public string Path { get; set; }

            public long MaxRetention { get; set; }
        }
    }
}
