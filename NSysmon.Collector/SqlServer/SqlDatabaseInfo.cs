using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

/// <remarks>
/// This code is derived from https://github.com/opserver/Opserver/tree/a170ea8bcda9f9e52d4aaff7339f3d198309369b
/// under "The MIT License (MIT)". Copyright (c) 2013 Stack Exchange Inc.
/// </remarks>
namespace NSysmon.Collector.SqlServer
{
    public class SqlDatabaseInfo : ISQLVersionedObject, IMonitorStatus
    {
        [JsonIgnore]
        public Version MinVersion { get { return SqlServerVersions.SQL2005.RTM; } }

        public string OverallStateDescription
        {
            get
            {
                if (IsReadOnly) return "Read-only";
                // TODO: Other statuses, e.g. Not Synchronizing
                return State.GetDescription();
            }
        }

        public MonitorStatus MonitorStatus
        {
            get
            {
                if (IsReadOnly)
                    return MonitorStatus.Warning;

                switch (State)
                {
                    case DatabaseStates.Restoring:
                    case DatabaseStates.Recovering:
                    case DatabaseStates.RecoveryPending:
                        return MonitorStatus.Unknown;
                    case DatabaseStates.Copying:
                        return MonitorStatus.Warning;
                    case DatabaseStates.Suspect:
                    case DatabaseStates.Emergency:
                    case DatabaseStates.Offline:
                        return MonitorStatus.Critical;
                    case DatabaseStates.Online:
                    default:
                        return MonitorStatus.Good;
                }
            }
        }
        public string MonitorStatusReason
        {
            get
            {
                if (IsReadOnly)
                    return "Database is read-only";

                switch (State)
                {
                    case DatabaseStates.Online:
                        return null;
                    default:
                        return "Database State: " + State.GetDescription();
                }
            }
        }

        private bool? _isSystemDatabase;
        public int Id { get; internal set; }
        public bool IsSystemDatabase
        {
            get { return (_isSystemDatabase ?? (_isSystemDatabase = SystemDatabaseNames.Contains(Name))).Value; }
        }
        public string Name { get; internal set; }
        public DatabaseStates State { get; internal set; }
        public CompatabilityLevels CompatbilityLevel { get; internal set; }
        public RecoveryModels RecoveryModel { get; internal set; }
        public PageVerifyOptions PageVerifyOption { get; internal set; }
        public LogReuseWaits LogReuseWait { get; internal set; }
        public Guid? ReplicaId { get; internal set; }
        public Guid? GroupDatabaseId { get; internal set; }
        public UserAccesses UserAccess { get; internal set; }
        public bool IsFullTextEnabled { get; internal set; }
        public bool IsReadOnly { get; internal set; }
        public bool IsReadCommittedSnapshotOn { get; internal set; }
        public bool IsBrokerEnabled { get; internal set; }
        public bool IsEncrypted { get; internal set; }
        public SnapshotIsolationStates SnapshotIsolationState { get; internal set; }
        public Containments? Containment { get; internal set; }
        public string LogVolumeId { get; internal set; }
        public double TotalSizeMB { get; internal set; }
        public double RowSizeMB { get; internal set; }
        public double StreamSizeMB { get; internal set; }
        public double TextIndexSizeMB { get; internal set; }
        public double? LogSizeMB { get; internal set; }
        public double? LogSizeUsedMB { get; internal set; }

        public double? LogPercentUsed { get { return LogSizeMB > 0 ? 100 * LogSizeUsedMB / LogSizeMB : null; } }

        public static List<string> SystemDatabaseNames = new List<string>
        {
            "master",
            "model",
            "msdb",
            "tempdb"
        };

        internal const string FetchSQL2012Columns = @"
       db.replica_id ReplicaId,
       db.group_database_id GroupDatabaseId,
       db.containment Containment, 
       v.LogVolumeId,
";
        internal const string FetchSQL2012Joins = @"
     Left Join (Select mf.database_id, vs.volume_id LogVolumeId
                  From sys.master_files mf
                       Cross Apply sys.dm_os_volume_stats(mf.database_id, mf.file_id) vs
                 Where type = 1) v On db.database_id = v.database_id
";

        internal const string FetchSQL = @"
Select db.database_id Id,
       db.name Name,
       db.state State,
       db.compatibility_level CompatibilityLevel,
       db.recovery_model RecoveryModel,
       db.page_verify_option PageVerifyOption,
       db.log_reuse_wait LogReuseWait,
       db.user_access UserAccess,
       db.is_fulltext_enabled IsFullTextEnabled,
       db.is_read_only IsReadOnly,
       db.is_read_committed_snapshot_on IsReadCommittedSnapshotOn,
       db.snapshot_isolation_state SnapshotIsolationState,
       db.is_broker_enabled IsBrokerEnabled,
       db.is_encrypted IsEncrypted, {0}
       (Cast(st.TotalSize As Float)*8)/1024 TotalSizeMB,
       (Cast(sr.RowSize As Float)*8)/1024 RowSizeMB,
       (Cast(ss.StreamSize As Float)*8)/1024 StreamSizeMB,
       (Cast(sti.TextIndexSize As Float)*8)/1024 TextIndexSizeMB,
       Cast(logs.cntr_value as Float)/1024 LogSizeMB,
       Cast(logu.cntr_value as Float)/1024 LogSizeUsedMB
From sys.databases db
     Left Join sys.dm_os_performance_counters logu On db.name = logu.instance_name And logu.counter_name LIKE N'Log File(s) Used Size (KB)%' 
     Left Join sys.dm_os_performance_counters logs On db.name = logs.instance_name And logs.counter_name LIKE N'Log File(s) Size (KB)%' 
     Left Join (Select database_id, Sum(Cast(size As Bigint)) TotalSize 
                  From sys.master_files 
              Group By database_id) st On db.database_id = st.database_id
     Left Join (Select database_id, Sum(Cast(size As Bigint)) RowSize 
                  From sys.master_files 
                 Where type = 0 
              Group By database_id) sr On db.database_id = sr.database_id
     Left Join (Select database_id, Sum(Cast(size As Bigint)) StreamSize 
                  From sys.master_files 
                 Where type = 2
              Group By database_id) ss On db.database_id = ss.database_id
     Left Join (Select database_id, Sum(Cast(size As Bigint)) TextIndexSize 
                  From sys.master_files 
                 Where type = 4
              Group By database_id) sti On db.database_id = sti.database_id {1}";

        public string GetFetchSQL(Version version)
        {
            if (version >= SqlServerVersions.SQL2012.RTM)
                return string.Format(FetchSQL, FetchSQL2012Columns, FetchSQL2012Joins);

            return string.Format(FetchSQL, "", "");
        }
    }
}
