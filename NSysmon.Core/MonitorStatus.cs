using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSysmon.Core
{
    /// <remarks>
    /// This code is derived from https://github.com/opserver/Opserver/tree/a170ea8bcda9f9e52d4aaff7339f3d198309369b
    /// under "The MIT License (MIT)". Copyright (c) 2013 Stack Exchange Inc.
    /// </remarks>
    public enum MonitorStatus
    {
        [Description("good")]
        Good = 0,
        [Description("unknown")]
        Unknown = 1,
        [Description("maintenance")]
        Maintenance = 2,
        [Description("warning")]
        Warning = 3,
        [Description("critical")]
        Critical = 4
    }

    /// <remarks>
    /// This code is derived from https://github.com/opserver/Opserver/tree/a170ea8bcda9f9e52d4aaff7339f3d198309369b
    /// under "The MIT License (MIT)". Copyright (c) 2013 Stack Exchange Inc.
    /// </remarks>
    public interface IMonitorStatus
    {
        MonitorStatus MonitorStatus { get; }
        string MonitorStatusReason { get; }
    }
}
