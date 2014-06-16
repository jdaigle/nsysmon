using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

/// <remarks>
/// This code is derived from https://github.com/opserver/Opserver/tree/a170ea8bcda9f9e52d4aaff7339f3d198309369b
/// under "The MIT License (MIT)". Copyright (c) 2013 Stack Exchange Inc.
/// </remarks>
namespace NSysmon.Collector.SqlServer
{
    public class SqlDatabaseVLFInfo : ISQLVersionedObject
    {
        [JsonIgnore]
        public Version MinVersion { get { return SqlServerVersions.SQL2005.RTM; } }

        public int DatabaseId { get; internal set; }
        public string DatabaseName { get; internal set; }
        public int VLFCount { get; internal set; }

        internal const string FetchSQL = @"
Create Table #VLFCounts (DatabaseId int, DatabaseName sysname, VLFCount int);
Create Table #vlfTemp (
    RecoveryUnitId int,
    FileId int, 
    FileSize nvarchar(255), 
    StartOffset nvarchar(255), 
    FSeqNo nvarchar(255), 
    Status int, 
    Parity int, 
    CreateLSN nvarchar(255)
);

IF EXISTS (SELECT * FROM fn_my_permissions(NULL, 'SERVER') WHERE permission_name = 'CONTROL SERVER')
BEGIN
Declare @sql nvarchar(max);
Set @sql = '';
Select @sql = @sql + ' Insert #vlfTemp Exec ' + QuoteName(name) + '.sys.sp_executesql N''DBCC LOGINFO WITH NO_INFOMSGS'';
  Insert #VLFCounts Select ' + Cast(database_id as nvarchar(10)) + ',''' + name + ''', Count(*) From #vlfTemp;
  Truncate Table #vlfTemp;'
  From sys.databases
  Where state <> 6; -- Skip OFFLINE databases as they cause errors
Exec sp_executesql @sql;
END

Select * From #VLFCounts;
Drop Table #VLFCounts;
Drop Table #vlfTemp;";

        public string GetFetchSQL(Version v)
        {
            if (v < SqlServerVersions.SQL2012.RTM)
                return FetchSQL.Replace("RecoveryUnitId int,", "");
            return FetchSQL;
        }
    }
}
