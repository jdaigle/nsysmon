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
    public class SqlDatabaseBackupInfo : ISQLVersionedObject
    {
        [JsonIgnore]
        public Version MinVersion { get { return SqlServerVersions.SQL2005.RTM; } }

        public int Id { get; internal set; }
        public string Name { get; internal set; }

        public DateTime? LastBackupStartDate { get; internal set; }
        public DateTime? LastBackupFinishDate { get; internal set; }
        public decimal? LastBackupSizeBytes { get; internal set; }
        public decimal? LastBackupCompressedSizeBytes { get; internal set; }
        public MediaDeviceTypes? LastBackupMediaDeviceType { get; internal set; }
        public string LastBackupLogicalDeviceName { get; internal set; }
        public string LastBackupPhysicalDeviceName { get; internal set; }

        public DateTime? LastFullBackupStartDate { get; internal set; }
        public DateTime? LastFullBackupFinishDate { get; internal set; }
        public decimal? LastFullBackupSizeBytes { get; internal set; }
        public decimal? LastFullBackupCompressedSizeBytes { get; internal set; }
        public MediaDeviceTypes? LastFullBackupMediaDeviceType { get; internal set; }
        public string LastFullBackupLogicalDeviceName { get; internal set; }
        public string LastFullBackupPhysicalDeviceName { get; internal set; }

        internal const string FetchSQL = @"
Select db.database_id Id,
       db.name Name, 
       lb.type LastBackupType,
       lb.backup_start_date LastBackupStartDate,
       lb.backup_finish_date LastBackupFinishDate,
       lb.backup_size LastBackupSizeBytes,
       lb.compressed_backup_size LastBackupCompressedSizeBytes,
       lbmf.device_type LastBackupMediaDeviceType,
       lbmf.logical_device_name LastBackupLogicalDeviceName,
       lbmf.physical_device_name LastBackupPhysicalDeviceName,
       fb.backup_start_date LastFullBackupStartDate,
       fb.backup_finish_date LastFullBackupFinishDate,
       fb.backup_size LastFullBackupSizeBytes,
       fb.compressed_backup_size LastFullBackupCompressedSizeBytes,
       fbmf.device_type LastFullBackupMediaDeviceType,
       fbmf.logical_device_name LastFullBackupLogicalDeviceName,
       fbmf.physical_device_name LastFullBackupPhysicalDeviceName
  From sys.databases db
       Left Outer Join (Select *
                          From (Select backup_set_id, 
                                       database_name,
                                       backup_start_date,
                                       backup_finish_date,
                                       backup_size,
                                       compressed_backup_size,
                                       media_set_id,
                                       type,
                                       Row_Number() Over(Partition By database_name Order By backup_start_date Desc) as rownum
                                  From msdb.dbo.backupset) b                                  
                           Where rownum = 1) lb 
         On lb.database_name = db.name
       Left Outer Join msdb.dbo.backupmediafamily lbmf
         On lb.media_set_id = lbmf.media_set_id And lbmf.media_count = 1
       Left Outer Join (Select *
                          From (Select backup_set_id, 
                                       database_name,
                                       backup_start_date,
                                       backup_finish_date,
                                       backup_size,
                                       compressed_backup_size,
                                       media_set_id,
                                       type,
                                       Row_Number() Over(Partition By database_name Order By backup_start_date Desc) as rownum
                                  From msdb.dbo.backupset
                                 Where type = 'D') b                                  
                           Where rownum = 1) fb 
         On fb.database_name = db.name
       Left Outer Join msdb.dbo.backupmediafamily fbmf
         On fb.media_set_id = fbmf.media_set_id And fbmf.media_count = 1";

        public string GetFetchSQL(Version v)
        {
            // Compressed backup info added in 2008
            if (v < SqlServerVersions.SQL2008.RTM)
            {
                return FetchSQL.Replace("compressed_backup_size,", "null compressed_backup_size,");
            }
            return FetchSQL;
        }
    }
}
