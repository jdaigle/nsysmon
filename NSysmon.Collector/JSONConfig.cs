using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSysmon.Collector
{
    public class JSONConfig
    {
        public JSONConfig()
        {
            nodes = new JSONConfigNode[0];
            syslog = new JSONConfigSyslog();
            http = new JSONConfigHttp();
        }

        public JSONConfigNode[] nodes { get; set; }
        public JSONConfigSyslog syslog { get; set; }
        public JSONConfigHttp http { get; set; }
    }

    public class JSONConfigNode
    {
        public string type { get; set; }
        public string name { get; set; }
        public string[] groups { get; set; }
        public object settings { get; set; }
    }

    public class JSONConfigSyslog
    {
        public JSONConfigSyslog()
        {
            listenerEnabled = true;
            listenerPort = 514;
        }

        public bool listenerEnabled { get; set; }
        public int listenerPort { get; set; }
    }

    public class JSONConfigHttp
    {
        public JSONConfigHttp()
        {
            listenerPort = 8080;
        }

        public int listenerPort { get; set; }
    }
}
