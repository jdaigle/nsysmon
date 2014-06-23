using System.Collections.Generic;

namespace NSysmon.Collector.HAProxy
{
    /// <summary>
    /// Represents an AHProxy backend for a proxy
    /// </summary>
    public class Backend : Item
    {
        public List<Server> Servers { get; internal set; }
    }
}