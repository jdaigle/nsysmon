using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSysmon.Core.WMI
{
    public class TcpipNetworkInterface
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Caption { get; set; }
        public UInt64 BytesReceivedPerSec { get; set; }
        public UInt64 BytesSentPerSec { get; set; }
        public UInt64 CurrentBandwidth { get; set; }
    }
}
