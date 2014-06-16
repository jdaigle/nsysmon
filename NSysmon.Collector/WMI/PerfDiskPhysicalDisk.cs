using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSysmon.Collector.WMI
{
    public class PerfDiskPhysicalDisk
    {
        public string Name { get; set; }
        public UInt32 AvgDiskSecPerRead { get; set; }
        public UInt32 AvgDiskSecPerWrite { get; set; }
        public UInt32 DiskReadsPerSec { get; set; }
        public UInt32 DiskWritesPerSec { get; set; }
    }
}
