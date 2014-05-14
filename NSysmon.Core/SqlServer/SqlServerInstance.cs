using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Dapper;

/// <remarks>
/// This code is derived from https://github.com/opserver/Opserver/tree/a170ea8bcda9f9e52d4aaff7339f3d198309369b
/// under "The MIT License (MIT)". Copyright (c) 2013 Stack Exchange Inc.
/// </remarks>
namespace NSysmon.Core.SqlServer
{
    public class SqlServerInstance : PollNode
    {
        public override string NodeType { get { return "Sql Server"; } }
        public override int MinSecondsBetweenPolls { get { return 10; } }

        private readonly SqlServerInstanceSettings settings;
        public string Name { get; private set; }
        protected string connectionString;
        public Version Version { get; internal set; }

        public SqlServerInstance(string nodeName, SqlServerInstanceSettings settings)
            : base(nodeName)
        {
            Version = new Version(); // default to 0.0
            this.Name = nodeName;
            this.settings = settings;
            this.connectionString = settings.ConnectionString; // TODO: default connection strings?
        }

        public override IEnumerable<PollNodeDataCache> DataCaches
        {
            get
            {
                yield return ServerProperties;
                yield return WindowsProperties;
                yield return Configuration;
                yield return Databases;
                yield return DatabaseFiles;
                yield return DatabaseBackups;
                yield return DatabaseVLFs;
                yield return CPUHistoryLastHour;
                yield return JobSummary;
                yield return PerfCounters;
                yield return MemoryClerkSummary;
                yield return TraceFlags;
            }
        }

        protected override IEnumerable<MonitorStatus> GetMonitorStatus()
        {
            if (Databases != null && Databases.ContainsCachedData)
            {
                yield return Databases.Data.GetWorstStatus();
            }
        }
        protected override string GetMonitorStatusReason()
        {
            return (Databases != null && Databases.ContainsCachedData) ? Databases.Data.GetReasonSummary() : null;
        }

        /// <summary>
        /// Gets a connection for this server - YOU NEED TO DISPOSE OF IT
        /// </summary>
        protected SqlConnection GetConnection(int timeout = 5000)
        {
            var conn = new SqlConnection(connectionString);
            conn.Open();
            return conn;
        }

        public string GetFetchSQL<T>() where T : ISQLVersionedObject
        {
            ISQLVersionedObject lookup;
            return ISQLVersionedObjectSingletons.TryGetValue(typeof(T), out lookup) ? lookup.GetFetchSQL(Version) : null;
        }

        public PollNodeDataCache<List<T>> SqlCacheList<T>(int cacheSeconds,
                                             int? cacheFailureSeconds = null,
                                             bool affectsStatus = true,
                                             [CallerMemberName] string memberName = "",
                                             [CallerFilePath] string sourceFilePath = "",
                                             [CallerLineNumber] int sourceLineNumber = 0)
           where T : class, ISQLVersionedObject
        {
            return new PollNodeDataCache<List<T>>(memberName, sourceFilePath, sourceLineNumber)
            {
                AffectsNodeStatus = affectsStatus,
                CacheForSeconds = cacheSeconds,
                CacheFailureForSeconds = cacheFailureSeconds,
                UpdateCachedData = UpdateFromSql(typeof(T).Name + "-List", conn => conn.Query<T>(GetFetchSQL<T>()).ToList())
            };
        }

        public Action<PollNodeDataCache<T>> UpdateFromSql<T>(string opName, Func<SqlConnection, T> getFromConnection) where T : class
        {
            return UpdateCachedData(description: "SQL Fetch: " + Name + ":" + opName,
                                    getData: () =>
                                    {
                                        using (var conn = GetConnection())
                                        {
                                            return getFromConnection(conn);
                                        }
                                    },
                                    addExceptionData: e => e.AddLoggedData("Server", Name));
        }

        private PollNodeDataCache<SqlServerWindowsInfo> _windowsProperties;
        public PollNodeDataCache<SqlServerWindowsInfo> WindowsProperties
        {
            get
            {
                return _windowsProperties ?? (_windowsProperties = new PollNodeDataCache<SqlServerWindowsInfo>()
                {
                    CacheForSeconds = 60,
                    UpdateCachedData = UpdateFromSql("dm_os_windows_info", conn =>
                    {
                        return conn.Query<SqlServerWindowsInfo>(SqlServerWindowsInfo.FetchSQL).FirstOrDefault();
                    })
                });
            }
        }

        private PollNodeDataCache<SqlServerProperties> _serverProperties;
        public PollNodeDataCache<SqlServerProperties> ServerProperties
        {
            get
            {
                return _serverProperties ?? (_serverProperties = new PollNodeDataCache<SqlServerProperties>()
                {
                    CacheForSeconds = 60,
                    UpdateCachedData = UpdateFromSql("Properties", conn =>
                    {
                        var result = conn.Query<SqlServerProperties>(SqlServerProperties.FetchSQL).FirstOrDefault();
                        if (result != null)
                        {
                            this.Version = result.ParsedVersion;
                        }
                        return result;
                    })
                });
            }
        }

        private PollNodeDataCache<List<SqlConfigurationOption>> _configuration;
        public PollNodeDataCache<List<SqlConfigurationOption>> Configuration
        {
            get
            {
                return _configuration ?? (_configuration = new PollNodeDataCache<List<SqlConfigurationOption>>
                {
                    CacheForSeconds = 2 * 60,
                    UpdateCachedData = UpdateFromSql("Configuration", conn =>
                    {
                        var result = conn.Query<SqlConfigurationOption>(SqlConfigurationOption.FetchSQL).ToList();
                        foreach (var r in result)
                        {
                            int defaultVal;
                            if (ConfigurationDefaults.TryGetValue(r.Name, out defaultVal))
                            {
                                r.Default = defaultVal;
                            }
                        }
                        return result;
                    })
                });
            }
        }

        private Dictionary<string, int> _configurationDefaults;
        public Dictionary<string, int> ConfigurationDefaults
        {
            get { return _configurationDefaults ?? (_configurationDefaults = SqlConfigurationOption.GetDefaults(this)); }
        }

        private PollNodeDataCache<List<SqlDatabaseInfo>> _databases;
        public PollNodeDataCache<List<SqlDatabaseInfo>> Databases
        {
            get { return _databases ?? (_databases = SqlCacheList<SqlDatabaseInfo>(5 * 60)); }
        }

        private PollNodeDataCache<List<SqlDatabaseFileInfo>> _databaseFiles;
        public PollNodeDataCache<List<SqlDatabaseFileInfo>> DatabaseFiles
        {
            get { return _databaseFiles ?? (_databaseFiles = SqlCacheList<SqlDatabaseFileInfo>(5 * 60)); }
        }

        private PollNodeDataCache<List<SqlDatabaseBackupInfo>> _databaseBackups;
        public PollNodeDataCache<List<SqlDatabaseBackupInfo>> DatabaseBackups
        {
            get { return _databaseBackups ?? (_databaseBackups = SqlCacheList<SqlDatabaseBackupInfo>(5 * 60)); }
        }

        private PollNodeDataCache<List<SqlDatabaseVLFInfo>> _databaseVLFs;
        public PollNodeDataCache<List<SqlDatabaseVLFInfo>> DatabaseVLFs
        {
            get { return _databaseVLFs ?? (_databaseVLFs = SqlCacheList<SqlDatabaseVLFInfo>(10 * 60, 60, affectsStatus: false)); }
        }

        private PollNodeDataCache<List<SqlCPUEvent>> _cpuHistoryLastHour;
        public PollNodeDataCache<List<SqlCPUEvent>> CPUHistoryLastHour
        {
            get
            {
                return _cpuHistoryLastHour ?? (_cpuHistoryLastHour = new PollNodeDataCache<List<SqlCPUEvent>>
                {
                    CacheForSeconds = 60,
                    UpdateCachedData = UpdateFromSql("CPUHistoryLastHour", conn =>
                    {
                        var sql = GetFetchSQL<SqlCPUEvent>();
                        var result = conn.Query<SqlCPUEvent>(sql, new { maxEvents = 60 })
                                         .OrderBy(e => e.EventTime)
                                         .ToList();
                        //CurrentCPUPercent = result.Count > 0 ? result.Last().ProcessUtilization : (int?)null;
                        return result;
                    })
                });
            }
        }

        private PollNodeDataCache<List<SqlJobInfo>> _jobSummary;
        public PollNodeDataCache<List<SqlJobInfo>> JobSummary
        {
            get { return _jobSummary ?? (_jobSummary = SqlCacheList<SqlJobInfo>(2 * 60)); }
        }

        private PollNodeDataCache<List<TraceFlagInfo>> _traceFlags;
        public PollNodeDataCache<List<TraceFlagInfo>> TraceFlags
        {
            get { return _traceFlags ?? (_traceFlags = SqlCacheList<TraceFlagInfo>(60 * 60)); }
        }

        private PollNodeDataCache<List<PerfCounterRecord>> _perfCounters;
        public PollNodeDataCache<List<PerfCounterRecord>> PerfCounters
        {
            get
            {
                return _perfCounters ?? (_perfCounters = new PollNodeDataCache<List<PerfCounterRecord>>
                {
                    CacheForSeconds = 20,
                    UpdateCachedData = UpdateFromSql("PerfCounters", conn =>
                    {
                        var sql = GetFetchSQL<PerfCounterRecord>();
                        return conn.Query<PerfCounterRecord>(sql, new { maxEvents = 60 }).ToList();
                    })
                });
            }
        }

        private PollNodeDataCache<List<SqlMemoryClerkSummaryInfo>> _memoryClerkSummary;
        public PollNodeDataCache<List<SqlMemoryClerkSummaryInfo>> MemoryClerkSummary
        {
            get { return _memoryClerkSummary ?? (_memoryClerkSummary = SqlCacheList<SqlMemoryClerkSummaryInfo>(30)); }
        }
    }
}
