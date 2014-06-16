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

        private void AddPerformanceCounter(string categoryName, string counterName, string instanceName)
        {
            if (!CounterExists(categoryName, counterName))
            {
                return;
            }
            if (!string.IsNullOrWhiteSpace(Machine))
            {
                this.Add(new PerformanceCounter(categoryName, counterName, instanceName, Machine));
            }
            else
            {
                this.Add(new PerformanceCounter(categoryName, counterName, instanceName));
            }
        }

        private string[] GetCounterCategoryInstanceNames(string category)
        {
            if (!string.IsNullOrWhiteSpace(Machine))
            {
                return new PerformanceCounterCategory(category, Machine).GetInstanceNames();
            }
            else
            {
                return new PerformanceCounterCategory(category).GetInstanceNames();
            }
        }

        private bool CategoryExists(string category)
        {
            if (!string.IsNullOrWhiteSpace(Machine))
            {
                return PerformanceCounterCategory.Exists(category, Machine);
            }
            else
            {
                return PerformanceCounterCategory.Exists(category);
            }
        }

        private bool CounterExists(string category, string counter)
        {
            if (!CategoryExists(category))
            {
                return false;
            }
            if (!string.IsNullOrWhiteSpace(Machine))
            {
                return PerformanceCounterCategory.CounterExists(counter, category, Machine);
            }
            else
            {
                return PerformanceCounterCategory.CounterExists(counter, category);
            }
        }

        private void SetupCounters()
        {
            this.AddPerformanceCounter("Memory", "Available MBytes", "");
            this.AddPerformanceCounter("Paging File", "% Usage", "_Total");
            this.AddPerformanceCounter("System", "Processor Queue Length", "");
            foreach (var instance in GetCounterCategoryInstanceNames("Processor"))
            {
                if (instance == "_Total")
                {
                    // only check real processors
                    continue;
                }
                this.AddPerformanceCounter("Processor", "% Processor Time", instance);
            }
            foreach (var instance in GetCounterCategoryInstanceNames("PhysicalDisk"))
            {
                if (instance == "_Total")
                {
                    // only check real disks
                    continue;
                }
                this.AddPerformanceCounter("PhysicalDisk", "Avg. Disk sec/Read", instance);
                this.AddPerformanceCounter("PhysicalDisk", "Avg. Disk sec/Write", instance);
                this.AddPerformanceCounter("PhysicalDisk", "Disk Reads/Sec", instance);
                this.AddPerformanceCounter("PhysicalDisk", "Disk Writes/Sec", instance);
            }
            foreach (var instance in GetCounterCategoryInstanceNames("Network Interface"))
            {
                this.AddPerformanceCounter("Network Interface", "Bytes Received/Sec", instance);
                this.AddPerformanceCounter("Network Interface", "Bytes Sent/Sec", instance);
            }
            if (IncludeSQLServerCounters)
            {
                this.AddPerformanceCounter("SQLServer:Buffer Manager", "Page life expectancy", "");
                this.AddPerformanceCounter("SQLServer:General Statistics", "User Connections", "");
                this.AddPerformanceCounter("SQLServer:Memory Manager", "Memory Grants Pending", "");
                this.AddPerformanceCounter("SQLServer:SQL Statistics", "Batch Requests/sec", "");
                this.AddPerformanceCounter("SQLServer:SQL Statistics", "SQL Compilations/sec", "");
                this.AddPerformanceCounter("SQLServer:SQL Statistics", "SQL Re-Compilations/sec", "");
            }
            foreach (var instance in GetCounterCategoryInstanceNames("ASP.NET Applications"))
            {
                if (instance == "__Total__")
                {
                    // only check real app pools
                    continue;
                }
                this.AddPerformanceCounter("ASP.NET Applications", "Requests/Sec", instance);
                this.AddPerformanceCounter("ASP.NET Applications", "Request Execution Time", instance);
                this.AddPerformanceCounter("ASP.NET Applications", "Requests In Application Queue", instance);
                this.AddPerformanceCounter("ASP.NET Applications", "Requests Executing", instance);
                this.AddPerformanceCounter("ASP.NET Applications", "Request Wait Time", instance);
            }
        }
    }
}
