using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSysmon.Collector.WMI
{
    public class TcpipNetworkInterface
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Caption { get; set; }
        public UInt64 CurrentBandwidth { get; set; }
    }
}
