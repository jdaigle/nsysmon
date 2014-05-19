using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace NSysmon.Core.WMI
{
    /// <remarks>
    /// This code is derived from https://github.com/opserver/Opserver/tree/a170ea8bcda9f9e52d4aaff7339f3d198309369b
    /// under "The MIT License (MIT)". Copyright (c) 2013 Stack Exchange Inc.
    /// </remarks>
    public class Instrumentation
    {
        public static QueryResult<T> Query<T>(string machineName, WMIPollingSettings wps, string query, Func<IEnumerable<ManagementObject>, IEnumerable<T>> conversion)
        {
            var timer = Stopwatch.StartNew();
            var scope = GetManagementScope(machineName, wps);
            var searcher = GetManagementObjectSearcher(machineName, query, scope);
            using (var results = searcher.Get())
            {
                var queryResults = results.Cast<ManagementObject>();
                timer.Stop();
                return new QueryResult<T>
                {
                    Duration = timer.Elapsed,
                    Data = conversion(queryResults).ToList()
                };
            }
        }

        public static ManagementObjectSearcher GetManagementObjectSearcher(string machineName, string query, ManagementScope scope)
        {
            var key = (machineName + query).ToLower();
            if (!ManagementObjectSearchers.ContainsKey(key))
            {
                var searcher = new ManagementObjectSearcher(scope, new ObjectQuery(query));
                ManagementObjectSearchers.TryAdd(key, searcher);
            }
            return ManagementObjectSearchers[key];
        }

        private static readonly ConcurrentDictionary<string, ManagementObjectSearcher> ManagementObjectSearchers = new ConcurrentDictionary<string, ManagementObjectSearcher>();

        public static ManagementScope GetManagementScope(string machineName, WMIPollingSettings wps)
        {
            if (!ManagementScopes.ContainsKey(machineName.ToLower()))
            {
                var newScope = new ManagementScope(string.Format(@"\\{0}\root\cimv2", machineName), GetConnectOptions(machineName, wps));
                ManagementScopes.TryAdd(machineName.ToLower(), newScope);
            }
            return ManagementScopes[machineName.ToLower()];
        }

        private static readonly ConcurrentDictionary<string, ManagementScope> ManagementScopes = new ConcurrentDictionary<string, ManagementScope>();

        public static ConnectionOptions GetConnectOptions(string machineName, WMIPollingSettings wps)
        {
            var co = new ConnectionOptions();
            if (machineName == Environment.MachineName)
            {
                return co;
            }
            switch (machineName)
            {
                case "localhost":
                case "127.0.0.1":
                    return co;
                default:
                    co = new ConnectionOptions
                    {
                        Authentication = AuthenticationLevel.Packet,
                        Timeout = new TimeSpan(0, 0, 30),
                        EnablePrivileges = true,
                    };
                    break;
            }
            if (wps != null && !string.IsNullOrWhiteSpace(wps.AuthUser) && !string.IsNullOrWhiteSpace(wps.AuthPassword))
            {
                co.Username = wps.AuthUser;
                co.Password = wps.AuthPassword;
            }
            if (wps != null && wps.QueryTimeout != default(int))
            {
                co.Timeout = new TimeSpan(0, 0, wps.QueryTimeout);
            }
            return co;
        }
    }
}
