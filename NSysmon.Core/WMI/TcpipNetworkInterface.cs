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
        public UInt32 BytesReceivedPerSec { get; set; }
        public UInt32 BytesSentPerSec { get; set; }
        public UInt32 CurrentBandwidth { get; set; }
    }
}
