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
    public class TraceFlagInfo : ISQLVersionedObject
    {
        // This likely works fine on 6+, need to test
        [JsonIgnore]
        public Version MinVersion { get { return SqlServerVersions.SQL2000.RTM; } }

        public int TraceFlag { get; internal set; }
        public bool Enabled { get; internal set; }
        public bool Global { get; internal set; }
        public int Session { get; internal set; }

        internal const string FetchSQL = @"
Declare @Flags Table(TraceFlag INT, Enabled BIT, Global BIT, Session INT);
Insert Into @Flags Exec('DBCC TRACESTATUS (-1) WITH NO_INFOMSGS');
Select * From @flags;";

        public string GetFetchSQL(Version v)
        {
            return FetchSQL;
        }
    }
}
