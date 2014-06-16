using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSysmon.Collector.WMI
{
    public class PerfOSSystem
    {
        public string Name { get; set; }
        public UInt32 ProcessorQueueLength { get; set; }
    }
}
