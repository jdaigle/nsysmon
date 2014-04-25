using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSysmon.Core.WMI
{
    public class PerfOSMemory
    {
        public UInt64 AvailableBytes { get; set; }
        public UInt64 AvailableMBytes { get; set; }
    }
}
