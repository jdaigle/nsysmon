﻿using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace NSysmon.Collector.HAProxy
{
    public class Proxy
    {
        [JsonIgnore]
        public HAProxyNode Instance { get; internal set; }
        public string Name { get; internal set; }
        //public string GroupName { get { return Instance.Group != null ? Instance.Group.Name : ""; } }
        //public string GroupLinkName { get { return string.Concat(Instance.Group != null ? Instance.Group.Name + "-" : "", Name); } }
        //public string InstanceLinkName { get { return string.Concat(Instance.Name, "-", Name); } }
        public Frontend Frontend { get; internal set; }
        public List<Server> Servers { get; internal set; }
        public Backend Backend { get; internal set; }
        public DateTime PollDate { get; internal set; }

        public IEnumerable<Item> AllStats
        {
            get
            {
                if(Frontend != null)
                    yield return Frontend;
                foreach (var s in Servers)
                    yield return s;
                if (Backend != null)
                    yield return Backend;
            }
        }

        public bool HasContent
        {
            get { return Frontend != null || Backend != null || (Servers != null && Servers.Count > 0); }
        }
        
        public bool HasFrontend { get { return Frontend != null; } }
        public bool HasServers { get { return Servers != null && Servers.Count > 0; } }
        public bool HasBackend { get { return Backend != null; } }

        private Item Primary { get { return (Item)Frontend ?? Backend; } }

        public string Status { get { return Primary.Status; } }
        public int LastStatusChangeSecondsAgo { get { return Primary.LastStatusChangeSecondsAgo; } }
        public long BytesIn { get { return Primary.BytesIn; } }
        public long BytesOut { get { return Primary.BytesIn; } }

        public int Order
        {
            get { return Frontend != null ? 0 : HasServers ? 1 : 2; }
        }

        public bool ShowQueue
        {
            get { return HasServers && Servers.Any(s => s.LimitSessions > 0 || s.LimitNewSessionPerSecond > 0); }
        }
        public bool ShowWarnings
        {
            get { return HasServers; }
        }
        public bool ShowServers
        {
            get { return HasServers; }
        }
        public bool ShowThrottle
        {
            get { return HasServers && Servers.Any(s => s.Throttle > 0); }
        }

        public string NiceName
        {
            get
            {
                //string result;
                //if (Current.Settings.HAProxy.Aliases.TryGetValue(Name, out result))
                //    return result;
                return Name;
            }
        }
    }
}