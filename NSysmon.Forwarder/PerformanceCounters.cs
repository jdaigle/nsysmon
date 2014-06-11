using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace NSysmon.Forwarder
{
    public class PerformanceCounters : List<PerformanceCounter>
    {
        public PerformanceCounters(string machine, bool includeSQLServerCounters = false)
        {
            this.Machine = machine;
            this.IncludeSQLServerCounters = includeSQLServerCounters;
            this.SetupCounters();
        }

        public string Machine { get; set; }
        public bool IncludeSQLServerCounters { get; set; }

        private void SetupCounters()
        {
            this.Add(new PerformanceCounter("Memory", "Available MBytes", "", Machine));
            this.Add(new PerformanceCounter("Paging File", "% Usage", "_Total", Machine));
            this.Add(new PerformanceCounter("System", "Processor Queue Length", "", Machine));
            foreach (var instance in new PerformanceCounterCategory("Processor", Machine).GetInstanceNames())
            {
                if (instance == "_Total")
                {
                    // only check real processors
                    continue;
                }
                this.Add(new PerformanceCounter("Processor", "% Processor Time", instance, Machine));
            }
            foreach (var instance in new PerformanceCounterCategory("PhysicalDisk", Machine).GetInstanceNames())
            {
                if (instance == "_Total")
                {
                    // only check real disks
                    continue;
                }
                this.Add(new PerformanceCounter("PhysicalDisk", "Avg. Disk sec/Read", instance, Machine));
                this.Add(new PerformanceCounter("PhysicalDisk", "Avg. Disk sec/Write", instance, Machine));
                this.Add(new PerformanceCounter("PhysicalDisk", "Disk Reads/Sec", instance, Machine));
                this.Add(new PerformanceCounter("PhysicalDisk", "Disk Writes/Sec", instance, Machine));
            }
            foreach (var instance in new PerformanceCounterCategory("Network Interface", Machine).GetInstanceNames())
            {
                this.Add(new PerformanceCounter("Network Interface", "Bytes Received/Sec", instance, Machine));
                this.Add(new PerformanceCounter("Network Interface", "Bytes Sent/Sec", instance, Machine));
                this.Add(new PerformanceCounter("Network Interface", "Current Bandwidth", instance, Machine));
            }
            if (IncludeSQLServerCounters)
            {
                this.Add(new PerformanceCounter("SQLServer:Buffer Manager", "Page life expectancy", "", Machine));
                this.Add(new PerformanceCounter("SQLServer:General Statistics", "User Connections", "", Machine));
                this.Add(new PerformanceCounter("SQLServer:Memory Manager", "Memory Grants Pending", "", Machine));
                this.Add(new PerformanceCounter("SQLServer:SQL Statistics", "Batch Requests/sec", "", Machine));
                this.Add(new PerformanceCounter("SQLServer:SQL Statistics", "SQL Compilations/sec", "", Machine));
                this.Add(new PerformanceCounter("SQLServer:SQL Statistics", "SQL Re-Compilations/sec", "", Machine));
            }
            foreach (var instance in new PerformanceCounterCategory("ASP.NET Applications", Machine).GetInstanceNames())
            {
                if (instance == "__Total__")
                {
                    // only check real app pools
                    continue;
                }
                this.Add(new PerformanceCounter("ASP.NET Applications", "Requests/Sec", instance, Machine));
                this.Add(new PerformanceCounter("ASP.NET Applications", "Request Execution Time", instance, Machine));
                this.Add(new PerformanceCounter("ASP.NET Applications", "Requests In Application Queue", instance, Machine));
                this.Add(new PerformanceCounter("ASP.NET Applications", "Requests Executing", instance, Machine));
                this.Add(new PerformanceCounter("ASP.NET Applications", "Request Wait Time", instance, Machine));
            }
        }
    }
}
