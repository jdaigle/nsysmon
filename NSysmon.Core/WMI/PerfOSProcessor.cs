using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSysmon.Core.WMI
{
    public class PerfOSProcessor
    {
        public string Name { get; set; }
        public UInt64 Utilization { get; set; }
    }
}
