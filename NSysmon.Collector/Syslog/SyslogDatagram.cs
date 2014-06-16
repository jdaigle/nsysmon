using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NSysmon.Collector.Syslog
{
    public class SyslogDatagram
    {
        public SyslogDatagram(string datagram, int severity, int facility, DateTime sentDateTime, string sourceIPAddress)
        {
            this.Datagram = datagram;
            this.Severity = severity;
            this.Facility = facility;
            this.SentDateTime = sentDateTime;
            this.SourceIPAddress = sourceIPAddress;
        }

        public string Datagram { get; private set; }
        public int Severity { get; private set; }
        public int Facility { get; private set; }
        public DateTime SentDateTime { get; private set; }
        public string SourceIPAddress { get; private set; }

        public virtual void Handle() { }
    }
}
