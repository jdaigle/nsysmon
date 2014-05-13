using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

/// <remarks>
/// This code is derived from https://github.com/opserver/Opserver/tree/a170ea8bcda9f9e52d4aaff7339f3d198309369b
/// under "The MIT License (MIT)". Copyright (c) 2013 Stack Exchange Inc.
/// </remarks>
namespace NSysmon.Core.SqlServer
{
    public class SqlCPUEvent : ISQLVersionedObject
    {
        [JsonIgnore]
        public Version MinVersion { get { return SqlServerVersions.SQL2005.RTM; } }

        public DateTime EventTime { get; internal set; }
        public int ProcessUtilization { get; internal set; }
        public int SystemIdle { get; internal set; }
        public int ExternalProcessUtilization { get { return 100 - SystemIdle - ProcessUtilization; } }

        private const string _fetchSQL = @"
Select Top (@maxEvents) 
	   DateAdd(s, (timestamp - (osi.cpu_ticks / Convert(Float, (osi.cpu_ticks / osi.ms_ticks)))) / 1000, GETUTCDATE()) AS EventTime,
	   Record.value('(./Record/SchedulerMonitorEvent/SystemHealth/SystemIdle)[1]', 'int') as SystemIdle,
	   Record.value('(./Record/SchedulerMonitorEvent/SystemHealth/ProcessUtilization)[1]', 'int') as ProcessUtilization
  From (Select timestamp, 
               convert(xml, record) As Record 
	      From sys.dm_os_ring_buffers 
		 Where ring_buffer_type = N'RING_BUFFER_SCHEDULER_MONITOR'
		   And record Like '%<SystemHealth>%') x
	    Cross Join sys.dm_os_sys_info osi
Order By timestamp Desc";

        public string GetFetchSQL(Version v)
        {
            return _fetchSQL;
        }
    }
}
