using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace NSysmon.Collector.SqlServer
{
    public class SqlDatabaseFileInfo : ISQLVersionedObject
    {
        [JsonIgnore]
        public Version MinVersion { get { return SqlServerVersions.SQL2008R2.RTM; } }

        public string DatabaseName { get; private set; }
        public int DatabaseId { get; private set; }
        public int FileId { get; private set; }
        public string FileName { get; private set; }
        public byte FileType { get; private set; }
        public string FileTypeDesc { get; private set; }
        public string FilePhysicalName { get; private set; }
        public byte FileState { get; private set; }
        public string FileStateDesc { get; private set; }
        public int FileSizeInPages { get; private set; }
        public int FileMaxSizeInPages { get; private set; }
        public int Growth { get; private set; }
        public bool IsPercentGrowth { get; private set; }
        public string LogicalVolumeName { get; private set; }
        public string VolumeMountPoint { get; private set; }
        public long VolumeTotalBytes { get; private set; }
        public long VolumeAvailableBytes { get; private set; }

        public const string FetchSQL = @"
SELECT 
    d.name AS DatabaseName
    , f.database_id AS DatabaseId
    , f.[file_id] AS FileId
    , f.name AS [FileName]
    , f.[type] AS FileType
    , f.type_desc AS FileTypeDesc
    , f.physical_name AS FilePhysicalName
    , f.[state] AS FileState
    , f.state_desc AS FileStateDesc
    , f.size AS FileSizeInPages
    , f.max_size AS FileMaxSizeInPages
    , f.growth AS Growth
    , f.is_percent_growth AS IsPercentGrowth
    , logical_volume_name AS LogicalVolumeName
    , volume_mount_point AS VolumeMountPoint
    , total_bytes AS VolumeTotalBytes
    , available_bytes AS VolumeAvailableBytes
FROM sys.master_files AS f
    INNER JOIN sys.databases as d on d.database_id = f.database_id
    CROSS APPLY sys.dm_os_volume_stats(f.database_id, f.file_id);
";

        public string GetFetchSQL(Version version)
        {
            return FetchSQL;
        }
    }
}
