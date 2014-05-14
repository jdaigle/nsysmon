# nsysmon

The .NET based Network Monitoring System

## Architecture

The NSysmon.Core library is the home of the `PollEngine` and designed around
the concept of a `PollNode` and a `DataCache`.

A PollNode is an abstract class which should represent
a thing like servers, database instances, services, or applications you wish
to monitor.

Each instance of a PollNode will have a collection of DataCaches.
A DataCache encapsulate two concepts:

1. A cached data object. This object represents a point-in-time snapshot of
   the database polled by this DataCache for a particular PollNode instance.
2. A cache update function. This function is called when the PollEngine whenever
   the cached data object needs to be updated or refreshed.

The data is meant to be used as an active point-in-time representation of your
systems to give your Ops team a consolidated overview all servers, databases,
and applications.

## Design Roadmap

The design is still in the very early prototype phase. Some of our ideas include:

* Expose an HTTP endpoint which can display simple debug information;
  such as the status the PollEngine, each PollNode and each DataCache.
* Persist DataCache objects to a database to record time-series data and
  produce trend graphs.
* API to expose the data to front-end dashboard and admin control panels.

## Windows WMI Security

* The User must be a member of the following builtin groups:
 * Distributed COM Users
 * Event Log Readers
 * Performance Log Users
 * Performance Monitor Users
* The User must also be granted "Remote Enable" permissions
  on the WMI Control (apply to \\root\CIMV2).
  See: http://msdn.microsoft.com/en-us/library/aa393266.aspx

## SQL Server Security

The NSysmon login needs the following roles and permissions on each SQL Server:

    use [master];
    CREATE LOGIN <USER OR role> FROM WINDOWS WITH DEFAULT_DATABASE=[master]
    GRANT VIEW ANY DATABASE TO <USER OR role>;
    GRANT VIEW ANY DEFINITION TO <USER OR role>;
    GRANT VIEW SERVER STATE TO <USER OR role>;
    USE [msdb];
    CREATE USER <USER OR role> FOR LOGIN <USER OR role>;
    EXEC sp_addrolemember N'db_datareader', N'<USER OR role>';
    EXEC sp_addrolemember N'SQLAgentOperatorRole', N'<USER OR role>';
    EXEC sp_addrolemember N'SQLAgentUserRole', N'<USER OR role>';
    EXEC sp_addrolemember N'SQLAgentReaderRole', N'<USER OR role>';
    GRANT EXECUTE ON [msdb].[dbo].[agent_datetime] TO <USER OR role>;

**Important Note**: In order to query _SqlDatabaseVLFInfo_, the login also needs
to be in the `sysadmin` role and have the `CONTROL SERVER` permission.

## Credits

Many aspects of the architecture, and several code files are derived
from the wonderful [Opserver](https://github.com/opserver/Opserver) project developed by Stack Exchange Inc.

* Most code is derived from this commit: [a170ea8bcda9](https://github.com/opserver/Opserver/tree/a170ea8bcda9f9e52d4aaff7339f3d198309369b)
* Their [MIT License](https://github.com/opserver/Opserver/blob/a170ea8bcda9f9e52d4aaff7339f3d198309369b/license.txt), Copyright (c) 2013 Stack Exchange Inc.