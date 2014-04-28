using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSysmon.Core
{
    public class PingResult
    {
        public string Status { get; set; }
        public long RoundtripTime { get; set; }
        public int Ttl { get; set; }
        public int BufferLength { get; set; }
    }
}
