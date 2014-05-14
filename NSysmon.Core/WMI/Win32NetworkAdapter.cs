using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSysmon.Core.WMI
{
    public class Win32NetworkAdapter
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string AdapterType { get; set; }
        public UInt16 AdapterTypeId { get; set; }
        public string MACAddress { get; set; }
        public string NetConnectionID { get; set; }
        public UInt16 NetConnectionStatus { get; set; }
        public bool NetEnabled { get; set; }
        public bool PhysicalAdapter { get; set; }
        public string ProductName { get; set; }
        public string Manufacturer { get; set; }
        public string TimeOfLastReset { get; set; } // "yyyyMMddHHmmss.ffffff-240"
    }
}
