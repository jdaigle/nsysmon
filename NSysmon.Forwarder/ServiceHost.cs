using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using log4net;

namespace NSysmon.Forwarder
{
    public class ServiceHost
    {
        private static readonly ILog log = LogManager.GetLogger("NSysmon.Forwarder");

        public void Stop()
        {
        }

        public void Start()
        {
            var task = Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    try
                    {
                        StartPerfmonCollectionLoop();
                    }
                    catch (Exception e)
                    {
                        log.Error("Error in Service Loop", e);
                        Thread.Sleep(5 * 1000); // wait 5 seconds before trying again
                    }
                }
            });
        }

        private static void StartPerfmonCollectionLoop()
        {
            var counters = new PerformanceCounters("");
            var syslogForwarder = new UdpClient();
            syslogForwarder.Connect(ConfigurationManager.AppSettings["forward_host"], int.Parse(ConfigurationManager.AppSettings["forward_port"]));
            var sw = Stopwatch.StartNew();
            while (true)
            {
                sw.Restart();
                foreach (var counter in counters)
                {
                    var nextValue = counter.NextValue();
                    var timestamp = DateTime.Now;
                    var datagram = GetSyslogDatagram(counter, nextValue, timestamp);
                    var bytesSend = syslogForwarder.Send(datagram, datagram.Length);
                    log.Debug("Sending " + datagram.Length + " bytes");
                    if (bytesSend != datagram.Length)
                    {
                        log.ErrorFormat("bytes sent " + bytesSend + " does not equal datagram length " + datagram.Length);
                    }
                }
                sw.Stop();
                log.Info(string.Format("Queried {0} counters in {1} ms", counters.Count, sw.Elapsed.TotalMilliseconds));
                Thread.Sleep(10 * 1000);
            }
        }

        private static byte[] GetSyslogDatagram(PerformanceCounter counter, float value, DateTime timestamp)
        {
            var header = string.Format("<134>{0} {1} {2}:", timestamp.ToString("MMM dd HH:mm:ss", CultureInfo.InvariantCulture), GetFQDN(), GetProcess());
            var perfcounter = string.Format("PC \"{0}\" \"{1}\" \"{2}\" \"{3}\"", counter.CategoryName, counter.CounterName, counter.InstanceName, value);
            return Encoding.ASCII.GetBytes(header + perfcounter);
        }

        private static string process;
        private static string GetProcess()
        {
            if (process == null)
            {
                process = string.Format("{0}[{1}]", Process.GetCurrentProcess().ProcessName, Process.GetCurrentProcess().Id);
            }
            return process;
        }

        private static string hostName;
        public static string GetFQDN()
        {
            if (hostName == null)
            {
                // see: http://stackoverflow.com/a/804719/507
                var domainName = IPGlobalProperties.GetIPGlobalProperties().DomainName;
                hostName = Dns.GetHostName();
                if (!hostName.Contains(domainName))            // if the hostname does not already include the domain name
                {
                    hostName = hostName + "." + domainName;   // add the domain name part
                }
            }
            return hostName;                              // return the fully qualified domain name
        }

        private static string GetCounterKey(PerformanceCounter counter)
        {
            return string.Format("{0}__{1}__{2}__{3}", counter.MachineName, counter.CategoryName, counter.CounterName, counter.InstanceName);
        }

        private static string GetServerKey(PerformanceCounter counter)
        {
            return string.Format("server__" + counter.MachineName);
        }
    }
}
