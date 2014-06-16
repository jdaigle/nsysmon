using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSysmon.Collector.Syslog
{
    public static class SyslogDatagramParser
    {
        public static SyslogDatagram Parse(string datagram, string sourceIPAddress)
        {
            if (datagram[0] != '<' || datagram.IndexOf(">") < 0)
            {
                return null;
            }
            int pri = ParsePriority(datagram);
            int pri_severity = 0;
            int pri_facility = 0;
            pri_facility = Math.DivRem(pri, 8, out pri_severity);
            var i = datagram.IndexOf(">");
            var datepart1 = ParseString(datagram, i, ' ', out i);
            var datepart2 = ParseString(datagram, i, ' ', out i);
            var datepart3 = ParseString(datagram, i, ' ', out i);
            var datagramDateTime = DateTime.ParseExact(datepart1 + " " + datepart2 + " " + datepart3, "MMM dd HH:mm:ss", DateTimeFormatInfo.InvariantInfo);

            // get type of message
            if (datagram.IndexOf("haproxy[", StringComparison.InvariantCultureIgnoreCase) > 0)
            {
                return ParseHAProxySyslogDatagram(datagram, pri_severity, pri_facility, datagramDateTime, sourceIPAddress);
            }
            else if (datagram.IndexOf("NSysmon.Forwarder[", StringComparison.InvariantCultureIgnoreCase) > 0)
            {
                return ParsePerformanceCounterDatagram(datagram, pri_severity, pri_facility, datagramDateTime, sourceIPAddress);
            }
            else
            {
                return null;
            }
        }

        public static PerformanceCounterDatagram ParsePerformanceCounterDatagram(string datagram, int severity, int facility, DateTime sentDateTime, string sourceIPAddress)
        {
            var parsedDatagram = new PerformanceCounterDatagram(datagram, severity, facility, sentDateTime, sourceIPAddress);
            var i = datagram.IndexOf("NSysmon.Forwarder[", StringComparison.InvariantCultureIgnoreCase);
            var header = datagram.Substring(0, i - 1);

            parsedDatagram.Node_Name = header.Substring(header.LastIndexOf(' ') + 1);
            if (parsedDatagram.Node_Name.Contains(':'))
            {
                // node name cannot contain ':' so this must not be it
                parsedDatagram.Node_Name = sourceIPAddress;
            }
            i = datagram.IndexOf("NSysmon.Forwarder[", StringComparison.InvariantCultureIgnoreCase);
            i += 7;
            parsedDatagram.Pid = ParseString(datagram, i, ']', out i);
            i += 2; // skip space
            i += 3; // skip "PC "
            parsedDatagram.PerformanceCounterCategory = ParseString(datagram, i, '"', out i);
            i += 2; // skip space and quote
            parsedDatagram.PerformanceCounterName = ParseString(datagram, i, '"', out i);
            i += 2; // skip space and quote
            parsedDatagram.PerformanceCounterInstance = ParseString(datagram, i, '"', out i);
            i += 2; // skip space and quote
            parsedDatagram.PerformanceCounterValue = ParseFloat(datagram, i, '"', out i);

            return parsedDatagram;
        }

        public static HAProxySyslogDatagram ParseHAProxySyslogDatagram(string datagram, int severity, int facility, DateTime sentDateTime, string sourceIPAddress)
        {
            var parsedDatagram = new HAProxySyslogDatagram(datagram, severity, facility, sentDateTime, sourceIPAddress);
            var i = datagram.IndexOf("haproxy[", StringComparison.InvariantCultureIgnoreCase);
            var header = datagram.Substring(0, i - 1);
            parsedDatagram.Node_Name = header.Substring(header.LastIndexOf(' ') + 1);
            if (parsedDatagram.Node_Name.Contains(':'))
            {
                // node name cannot contain ':' so this must not be it
                parsedDatagram.Node_Name = sourceIPAddress;
            }
            i = datagram.IndexOf("haproxy[", StringComparison.InvariantCultureIgnoreCase);
            i += 7;
            parsedDatagram.Pid = ParseString(datagram, i, ']', out i);
            i += 2; // skip space
            parsedDatagram.Client_IP = ParseString(datagram, i, ':', out i);
            parsedDatagram.Client_Port = ParseInt(datagram, i, ' ', out i);
            i += 1; // skip [
            var accept_date_s = ParseString(datagram, i, ']', out i);
            parsedDatagram.Accept_Date = DateTime.ParseExact(accept_date_s, "dd/MMM/yyyy:HH:mm:ss.fff", CultureInfo.InvariantCulture);
            i += 1; // skip space
            parsedDatagram.Frontend_Name = ParseString(datagram, i, ' ', out i);
            parsedDatagram.Backend_Name = ParseString(datagram, i, '/', out i);
            parsedDatagram.Server_Name = ParseString(datagram, i, ' ', out i);
            parsedDatagram.Tq = ParseInt(datagram, i, '/', out i);
            parsedDatagram.Tw = ParseInt(datagram, i, '/', out i);
            parsedDatagram.Tc = ParseInt(datagram, i, '/', out i);
            parsedDatagram.Tr = ParseInt(datagram, i, '/', out i);
            parsedDatagram.Tt = ParseInt(datagram, i, ' ', out i);
            parsedDatagram.Status_Code = ParseInt(datagram, i, ' ', out i);
            parsedDatagram.Bytes_Read = ParseInt(datagram, i, ' ', out i);
            parsedDatagram.Captured_Request_Cookie = ParseString(datagram, i, ' ', out i);
            parsedDatagram.Captured_Response_Cookie = ParseString(datagram, i, ' ', out i);
            parsedDatagram.Termination_State = ParseString(datagram, i, ' ', out i);
            if (parsedDatagram.Termination_State.Length > 0)
            {
                parsedDatagram.Termination_State_SessionErrorCode = parsedDatagram.Termination_State[0];
                if (parsedDatagram.Termination_State.Length > 1)
                {
                    parsedDatagram.Termination_State_SessionStateCode = parsedDatagram.Termination_State[1];
                    if (parsedDatagram.Termination_State.Length > 2)
                    {
                        parsedDatagram.Termination_State_PersistenceCookieCode = parsedDatagram.Termination_State[2];
                        if (parsedDatagram.Termination_State.Length > 3)
                        {
                            parsedDatagram.Termination_State_PersistenceCookieOpCode = parsedDatagram.Termination_State[3];
                        }
                    }
                }
            }
            parsedDatagram.Actconn = ParseInt(datagram, i, '/', out i);
            parsedDatagram.Feconn = ParseInt(datagram, i, '/', out i);
            parsedDatagram.Beconn = ParseInt(datagram, i, '/', out i);
            parsedDatagram.Srv_conn = ParseInt(datagram, i, '/', out i);
            parsedDatagram.Retries = ParseInt(datagram, i, ' ', out i);
            parsedDatagram.Srv_Queue = ParseInt(datagram, i, '/', out i);
            parsedDatagram.Backend_Queue = ParseInt(datagram, i, ' ', out i);
            parsedDatagram.Captured_Request_Headers = string.Empty;
            if (datagram[++i] == '{')
            {
                parsedDatagram.Captured_Request_Headers = ParseString(datagram, i, '}', out i);
                if (!string.IsNullOrWhiteSpace(parsedDatagram.Captured_Request_Headers))
                {
                    var parts = parsedDatagram.Captured_Request_Headers.Split(new char[] { '|' }, StringSplitOptions.None);
                    parsedDatagram.Referer = parts[0];
                    parsedDatagram.User_Agent = parts[1];
                    parsedDatagram.Host = parts[2];
                    parsedDatagram.X_Forwarded_For = parts[3];
                    parsedDatagram.Accept_Encoding = parts[4];
                }
                i += 1;
            }
            parsedDatagram.Captured_Response_Headers = string.Empty;
            if (datagram[++i] == '{')
            {
                parsedDatagram.Captured_Response_Headers = ParseString(datagram, i, '}', out i);
                if (!string.IsNullOrWhiteSpace(parsedDatagram.Captured_Response_Headers))
                {
                    var parts = parsedDatagram.Captured_Response_Headers.Split(new char[] { '|' }, StringSplitOptions.None);
                    parsedDatagram.Content_Encoding = parts[0];
                }
                i += 1;
            }
            i += 1;
            parsedDatagram.HTTP_Request = string.Empty;
            parsedDatagram.HTTP_Request_Method = string.Empty;
            while (++i < datagram.Length && datagram[i] != ' ')
            {
                parsedDatagram.HTTP_Request += datagram[i];
                parsedDatagram.HTTP_Request_Method += datagram[i];
            }
            parsedDatagram.HTTP_Request += ' ';
            parsedDatagram.HTTP_Request_URL = string.Empty;
            parsedDatagram.HTTP_Request_Query = string.Empty;
            while (++i < datagram.Length && datagram[i] != ' ')
            {
                parsedDatagram.HTTP_Request += datagram[i];
                parsedDatagram.HTTP_Request_URL += datagram[i];
            }
            if (parsedDatagram.HTTP_Request_URL.Contains('?'))
            {
                var parts = parsedDatagram.HTTP_Request_URL.Split(new char[] { '?' }, 2, StringSplitOptions.None);
                parsedDatagram.HTTP_Request_URL = parts[0];
                parsedDatagram.HTTP_Request_Query = parts[1];
            }
            parsedDatagram.HTTP_Request += ' ';
            parsedDatagram.HTTP_Request_Version = string.Empty;
            while (++i < datagram.Length && datagram[i] != '"')
            {
                parsedDatagram.HTTP_Request += datagram[i];
                parsedDatagram.HTTP_Request_Version += datagram[i];
            }
            return parsedDatagram;
        }

        public static int ParsePriority(string datagram)
        {
            int i = 0;
            var pri = ParseString(datagram, i, '>', out i);
            return int.Parse(pri);
        }

        public static string ParseString(string datagram, int start_i, char endChar, out int end_i)
        {
            var s = string.Empty;
            var i = start_i;
            while (datagram[++i] != endChar)
            {
                s += datagram[i];
            }
            end_i = i;
            return s;
        }

        public static int ParseInt(string datagram, int start_i, char endChar, out int end_i)
        {
            var i = start_i;
            var s = ParseString(datagram, start_i, endChar, out i);
            end_i = i;
            return int.Parse(s);
        }

        public static float ParseFloat(string datagram, int start_i, char endChar, out int end_i)
        {
            var i = start_i;
            var s = ParseString(datagram, start_i, endChar, out i);
            end_i = i;
            float d = 0;
            if (!float.TryParse(s, out d))
            {
                float.Parse(s);
            }
            return d;
        }
    }
}
