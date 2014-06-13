using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSysmon.Core.Syslog
{
    public class PerformanceCounterDatagram : SyslogDatagram
    {
        public PerformanceCounterDatagram(string datagram, int severity, int facility, DateTime sentDateTime, string sourceIPAddress)
            : base(datagram, severity, facility, sentDateTime, sourceIPAddress) { }

        public string Node_Name { get; set; }
        public string Pid { get; set; }

        public string PerformanceCounterCategory { get; set; }
        public string PerformanceCounterName { get; set; }
        public string PerformanceCounterInstance { get; set; }
        public float PerformanceCounterValue { get; set; }

        public override void Handle()
        {
            base.Handle();
            PerfMon.Instance.Publish(Node_Name, this.SentDateTime, PerformanceCounterCategory, PerformanceCounterName, PerformanceCounterInstance, PerformanceCounterValue);
        }
    }
}