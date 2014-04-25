using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSysmon.Core.WMI
{
    public class WindowsServerNodeSettings
    {
        public string Host { get; set; }
        public WMIPollingSettings WMIPollingSettings { get; set; }
    }
}
