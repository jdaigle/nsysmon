using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSysmon.Core.WMI
{
    public class Win32Volume
    {
        public string Name { get; set; }
        public string Label { get; set; }
        public UInt32 DriveType { get; set; }
        public bool IsLocalDisk { get { return DriveType == 3; } }
        public string DriveLetter { get; set; }
        public string FileSystem { get; set; }
        public UInt64 Capacity { get; set; }
        public UInt64 FreeSpace { get; set; }
    }
}
