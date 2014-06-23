using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSysmon.Collector.HAProxy
{
    public class HAProxyNodeSettings
    {
        public HAProxyNodeSettings()
        {
            QueryTimeout = 3000;
        }

        public string Url { get; set; }
        public string Description { get; set; }
        public int QueryTimeout { get; set; }
    }
}
