using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace NSysmon.Forwarder
{
    public class ServiceHost
    {
        public void Stop()
        {
        }

        public void Start()
        {
            System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                var counters = new PerformanceCounters("odysseus", includeSQLServerCounters: true);
                while (true)
                {
                    Console.Clear();
                    foreach (var counter in counters)
                    {                        
                        var nextValue = counter.NextValue();
                        var timestamp = DateTime.UtcNow;
                        var key = GetCounterKey(counter);
                        var value = Newtonsoft.Json.JsonConvert.SerializeObject(new CounterTimestamp(timestamp, nextValue));
                        Console.WriteLine(string.Format("{0} - {1} - {2} - {3}", counter.CategoryName, counter.CounterName, counter.InstanceName.Length > 10 ? counter.InstanceName.Substring(0, 10) : counter.InstanceName, nextValue));
                    }
                    Thread.Sleep(10 * 1000);
                }
            });
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
