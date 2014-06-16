using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSysmon.Collector.WMI
{
    public class Win32ComputerSystem
    {
        // From Win32_ComputerSystem
        public string Name { get; set; }
        public string Domain { get; set; }
        public string Manufacturer { get; set; }
        public string Model { get; set; }
        public UInt64 TotalPhysicalMemory { get; set; }

        // From Win32_OperatingSystem
        public string OSVersion { get; set; }
        public string OSBuildNumber { get; set; }
    }
}
