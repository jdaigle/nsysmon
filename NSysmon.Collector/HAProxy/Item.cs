﻿using System;
using System.Text.RegularExpressions;

namespace NSysmon.Collector.HAProxy
{
    public class Item
    {
        /// <summary>
        /// pxname: proxy name
        /// </summary>
        [Stat("pxname", 0)]
        public string ProxyName { get; internal set; }

        /// <summary>
        /// svname: service name (Frontend for Frontend, BACKEND for backend, any name for server)
        /// </summary>
        [Stat("svname", 1)]
        public string ServerName { get; internal set; }

        /// <summary>
        /// qcur: current queued requests
        /// </summary>
        [Stat("qcur", 2)]
        public int CurrentQueue { get; internal set; }

        /// <summary>
        /// qmax: max queued requests
        /// </summary>
        [Stat("qmax", 3)]
        public int MaxQueue { get; internal set; }

        /// <summary>
        /// qlimit: queue limit
        /// </summary>
        [Stat("qlimit", 25)]
        public int? LimitQueue { get; internal set; }

        /// <summary>
        /// scur: current sessions
        /// </summary>
        [Stat("scur", 4)]
        public int CurrentSessions { get; internal set; }

        /// <summary>
        /// smax: max sessions
        /// </summary>
        [Stat("smax", 5)]
        public int MaxSessions { get; internal set; }

        /// <summary>
        /// slim: sessions limit
        /// </summary>
        [Stat("slim", 6)]
        public int? LimitSessions { get; internal set; }

        /// <summary>
        /// stot: total sessions
        /// </summary>
        [Stat("stot", 7)]
        public long TotalSessions { get; internal set; }

        /// <summary>
        /// bin: bytes in
        /// </summary>
        [Stat("bin", 8)]
        public long BytesIn { get; internal set; }

        /// <summary>
        /// bout: bytes out
        /// </summary>
        [Stat("bout", 9)]
        public long BytesOut { get; internal set; }

        /// <summary>
        /// dreq: denied requests
        /// </summary>
        [Stat("dreq", 10)]
        public long DeniedRequests { get; internal set; }

        /// <summary>
        /// dresp: denied responses
        /// </summary>
        [Stat("dresp", 11)]
        public long DeniedResponses { get; internal set; }

        /// <summary>
        /// ereq: request errors
        /// </summary>
        [Stat("ereq", 12)]
        public long RequestErrors { get; internal set; }

        /// <summary>
        /// econ: connection errors
        /// </summary>
        [Stat("econ", 13)]
        public long ConnectionErrors { get; internal set; }

        /// <summary>
        /// eresp: response errors (among which srv_abrt)
        /// </summary>
        [Stat("eresp", 14)]
        public long ResponseErrors { get; internal set; }

        /// <summary>
        /// wretr: retries (warning)
        /// </summary>
        [Stat("wretr", 15)]
        public long WarningRetries { get; internal set; }

        /// <summary>
        /// wredis: redispatches (warning)
        /// </summary>
        [Stat("wredis", 16)]
        public long WarningRedisPatches { get; internal set; }

        /// <summary>
        /// status: status (UP/DOWN/NOLB/MAINT/MAINT(via)...)
        /// </summary>
        [Stat("status", 17)]
        public string Status { get; internal set; }

        /// <summary>
        /// weight: server weight (server), total weight (backend)
        /// </summary>
        [Stat("weight", 18)]
        public int Weight { get; internal set; }

        /// <summary>
        /// act: server is active (server), number of active servers (backend)
        /// </summary>
        [Stat("act", 19)]
        public int Active { get; internal set; }

        /// <summary>
        /// bck: server is backup (server), number of backup servers (backend)
        /// </summary>
        [Stat("bck", 20)]
        public int Backup { get; internal set; }

        /// <summary>
        /// chkfail: number of failed checks
        /// </summary>
        [Stat("chkfail", 21)]
        public long? CheckFails { get; internal set; }

        /// <summary>
        /// chkdown: number of UP->DOWN transitions
        /// </summary>
        [Stat("chkdown", 22)]
        public long UpDownTransitions { get; internal set; }

        /// <summary>
        /// lastchg: last status change (in seconds)
        /// </summary>
        [Stat("lastchg", 23)]
        public int LastStatusChangeSecondsAgo { get; internal set; }

        /// <summary>
        /// lastchg: last status change (in seconds) - as DateTime
        /// </summary>
        public DateTime LastStatusChange { get { return DateTime.UtcNow.AddSeconds(-LastStatusChangeSecondsAgo); } }

        /// <summary>
        /// lastchg: last status change (in seconds) - as TimeSpan
        /// </summary>
        public TimeSpan LastStatusDuration { get { return TimeSpan.FromSeconds(LastStatusChangeSecondsAgo); } }

        /// <summary>
        /// downtime: total downtime (in seconds)
        /// </summary>
        [Stat("downtime", 24)]
        public int DowntimeSeconds { get; internal set; }

        /// <summary>
        /// pid: process id (0 for first instance, 1 for second, ...)
        /// </summary>
        [Stat("pid", 26)]
        public int ProcessId { get; internal set; }

        /// <summary>
        /// iid: unique proxy id
        /// </summary>
        [Stat("iid", 27)]
        public int UniqueProxyId { get; internal set; }

        /// <summary>
        /// sid: service id (unique inside a proxy)
        /// </summary>
        [Stat("sid", 28)]
        public int ServiceId { get; internal set; }

        /// <summary>
        /// throttle: warm up status
        /// </summary>
        [Stat("throttle", 29)]
        public int Throttle { get; internal set; }

        /// <summary>
        /// lbtot: total number of times a server was selected
        /// </summary>
        [Stat("lbtot", 30)]
        public long LoadBalanceTotal { get; internal set; }

        /// <summary>
        /// tracked: id of proxy/server if tracking is enabled
        /// </summary>
        [Stat("tracked", 31)]
        public int? TrackedId { get; internal set; }

        /// <summary>
        /// type (0=Frontend, 1=backend, 2=server, 3=socket)
        /// </summary>
        [Stat("type", 32)]
        public int TypeId { get; internal set; }

        /// <summary>
        /// type: Frontend, Backend, Server, Socket
        /// </summary>
        public StatusType Type { get { return (StatusType)TypeId; } }

        /// <summary>
        /// rate: number of sessions per second over last elapsed second
        /// </summary>
        [Stat("rate", 33)]
        public int CurrentNewSessionsPerSecond { get; internal set; }

        /// <summary>
        /// rate_lim: limit on new sessions per second
        /// </summary>
        [Stat("rate_lim", 34)]
        public int LimitNewSessionPerSecond { get; internal set; }

        /// <summary>
        /// rate_max: max number of new sessions per second
        /// </summary>
        [Stat("rate_max", 35)]
        public int MaxNewSessionPerSecond { get; internal set; }

        /// <summary>
        /// check_status: status of last health check, one of:
        /// UNK     -> unknown
        /// INI     -> initializing
        /// SOCKERR -> socket error
        /// L4OK    -> check passed on layer 4, no upper layers testing enabled
        /// L4TMOUT -> layer 1-4 timeout
        /// L4CON   -> layer 1-4 connection problem, for example
        ///            "Connection refused" (tcp rst) or "No route to host" (icmp)
        /// L6OK    -> check passed on layer 6
        /// L6TOUT  -> layer 6 (SSL) timeout
        /// L6RSP   -> layer 6 invalid response - protocol error
        /// L7OK    -> check passed on layer 7
        /// L7OKC   -> check conditionally passed on layer 7, for example 404 with
        ///            disable-on-404
        /// L7TOUT  -> layer 7 (HTTP/SMTP) timeout
        /// L7RSP   -> layer 7 invalid response - protocol error
        /// L7STS   -> layer 7 response error, for example HTTP 5xx
        /// </summary>
        [Stat("check_status", 36)]
        public string CheckStatusString { get; internal set; }

        /// <summary>
        /// check_code: layer5-7 code, if available
        /// </summary>
        [Stat("check_code", 37)]
        public int CheckCode { get; internal set; }

        /// <summary>
        /// check_duration: time in ms took to finish last health check
        /// </summary>
        [Stat("check_duration", 38)]
        public int CheckDurationMiliseconds { get; internal set; }

        /// <summary>
        /// hrsp_1xx: http responses with 1xx code
        /// </summary>
        [Stat("hrsp_1xx", 39)]
        public long Http100LevelResponses { get; internal set; }

        /// <summary>
        /// hrsp_2xx: http responses with 2xx code
        /// </summary>
        [Stat("hrsp_2xx", 40)]
        public long Http200LevelResponses { get; internal set; }

        /// <summary>
        /// hrsp_3xx: http responses with 3xx code
        /// </summary>
        [Stat("hrsp_3xx", 41)]
        public long Http300LevelResponses { get; internal set; }

        /// <summary>
        /// hrsp_4xx: http responses with 4xx code
        /// </summary>
        [Stat("hrsp_4xx", 42)]
        public long Http400LevelResponses { get; internal set; }

        /// <summary>
        /// hrsp_5xx: http responses with 5xx code
        /// </summary>
        [Stat("hrsp_5xx", 43)]
        public long Http500LevelResponses { get; internal set; }

        /// <summary>
        /// hrsp_other: http responses with other codes (protocol error)
        /// </summary>
        [Stat("hrsp_other", 44)]
        public long HttpOtherLevelResponses { get; internal set; }

        /// <summary>
        /// hanafail: failed health checks details
        /// </summary>
        [Stat("hanafail", 45)]
        public string FailedHealthCheckDetails { get; internal set; }

        /// <summary>
        /// req_rate: HTTP requests per second over last elapsed second
        /// </summary>
        [Stat("req_rate", 46)]
        public int CurrentRequestsPerSecond { get; internal set; }

        /// <summary>
        /// req_rate_max: max number of HTTP requests per second observed
        /// </summary>
        [Stat("req_rate_max", 47)]
        public int MaxRequestsPerSecond { get; internal set; }

        /// <summary>
        /// req_tot: total number of HTTP requests received
        /// </summary>
        [Stat("req_tot", 48)]
        public long TotalRequests { get; internal set; }

        /// <summary>
        /// cli_abrt: number of data transfers aborted by the client
        /// </summary>
        [Stat("cli_abrt", 49)]
        public long ClientTransferAborts { get; internal set; }

        /// <summary>
        /// srv_abrt: number of data transfers aborted by the server (inc. in eresp)
        /// </summary>
        [Stat("srv_abrt", 50)]
        public long ServerTransferAborts { get; internal set; }

        /// <summary>
        /// number of HTTP response bytes fed to the compressor
        /// </summary>
        [Stat("comp_in", 51)]
        public long comp_in { get; internal set; }

        /// <summary>
        /// number of HTTP response bytes emitted by the compressor
        /// </summary>
        [Stat("comp_out", 52)]
        public long comp_out { get; internal set; }

        /// <summary>
        /// number of bytes that bypassed the HTTP compressor (CPU/BW limit)
        /// </summary>
        [Stat("comp_byp", 53)]
        public long comp_byp { get; internal set; }

        /// <summary>
        /// number of HTTP responses that were compressed
        /// </summary>
        [Stat("comp_rsp", 54)]
        public long comp_rsp { get; internal set; }

        /// <summary>
        /// number of seconds since last session assigned to server/backend
        /// </summary>
        [Stat("lastsess", 55)]
        public long SecondsSinceLastSession { get; internal set; }

        /// <summary>
        /// last health check contents or textual error
        /// </summary>
        [Stat("last_chk", 56)]
        public string LastHealthCheck { get; internal set; }

        /// <summary>
        /// last agent check contents or textual error
        /// </summary>
        [Stat("last_agt", 57)]
        public string LastAgentCheck { get; internal set; }

        /// <summary>
        /// the average queue time in ms over the 1024 last requests
        /// </summary>
        [Stat("qtime", 58)]
        public long AverageQueueTimeMilliseconds { get; internal set; }

        /// <summary>
        /// the average connect time in ms over the 1024 last requests
        /// </summary>
        [Stat("ctime", 59)]
        public long AverageConnectTimeMilliseconds { get; internal set; }

        /// <summary>
        /// the average response time in ms over the 1024 last requests (0 for TCP)
        /// </summary>
        [Stat("rtime", 60)]
        public long AverageResponseTimeMilliseconds { get; internal set; }

        /// <summary>
        /// the average total session time in ms over the 1024 last requests
        /// </summary>
        [Stat("ttime", 61)]
        public long AverageTotalSessionTimeMilliseconds { get; internal set; }

        /// <summary>
        /// The raw CSV stats line
        /// </summary>
        public string RawData { get; internal set; }

        /// <summary>
        /// Position of the "type" stat, which determines if this is a Backend, Frontend, Server or Socket
        /// </summary>
        private const int _typePosition = 32;

        private static readonly char[] commaSplit = new char[] { ',' };
        public static Item FromLine(string csvLine)
        {
            //Parse the line into an array (CSV format)
            string[] stats = csvLine.Split(commaSplit, StringSplitOptions.None);

            Item result = GetStatOfType(stats[_typePosition]);
            result.RawData = csvLine;

            for (var i = 0; i < StatProperty.AllOrdered.Count; i++)
            {
                //Get the stat from the split array
                var statText = stats[i];
                //If it's empty, skip it
                if(string.IsNullOrEmpty(statText)) continue;

                //Get the property info for this position
                var propInfo = StatProperty.AllOrdered[i].PropertyInfo;
                
                //Get the property type
                var type = propInfo.PropertyType;
                //Cast to the underlying type for nullables
                if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                    type = type.GetGenericArguments()[0];

                //Set the property on the result
                propInfo.SetValue(result, Convert.ChangeType(statText, type), null);
            }
            return result;
        }

        private static Item GetStatOfType(string typeNum)
        {
            var type = (StatusType)Int32.Parse(typeNum);
            switch (type)
            {
                case StatusType.Frontend:
                    return new Frontend();
                case StatusType.Backend:
                    return new Backend();
                case StatusType.Server:
                    return new Server();
                default:
                    throw new NotImplementedException("What the hell are you using sockets for?");
            }
        }

        public bool IsFrontend { get { return Type == StatusType.Frontend; } }
        public bool IsServer { get { return Type == StatusType.Server; } }
        public bool IsSocket { get { return Type == StatusType.Socket; } }
        public bool IsBackend { get { return Type == StatusType.Backend; } }

        public override string ToString()
        {
            return RawData;
        }

        private static readonly Regex _upGoingDown = new Regex(@"UP \d+/\d+", RegexOptions.Compiled);
        private static readonly Regex _downGoingUp = new Regex(@"DOWN \d+/\d+", RegexOptions.Compiled);

        public virtual string Description
        {
            get { return Type == StatusType.Server ? ServerName : Type.ToString(); }
        }

        public bool IsChecked { get { return Status != "no check"; } }
        public bool IsActive { get { return Active == 1; } }
        public bool IsBackup { get { return Backup == 1; } }

        private ProxyServerStatus? _proxyServerStatus;
        public ProxyServerStatus ProxyServerStatus
        {
            get { return _proxyServerStatus ?? (_proxyServerStatus = GetProxyServerStatus()).Value; }
        }

        private ProxyServerStatus GetProxyServerStatus()
        {
            // Check for the UP n/N format, which is a server up but failed the last n checks
            if (_upGoingDown.IsMatch(Status))
                return IsActive ? ProxyServerStatus.ActiveUpGoingDown : ProxyServerStatus.BackupUpGoingDown;
            // Check for the DOWN n/N format, which is a server down but passed the last n checks
            if (_downGoingUp.IsMatch(Status))
                return IsActive ? ProxyServerStatus.ActiveDownGoingUp : ProxyServerStatus.BackupDownGoingUp;

            switch (Status)
            {
                // Server is fully up
                case "UP":
                    return IsActive ? ProxyServerStatus.ActiveUp : ProxyServerStatus.BackupUp;
                case "OPEN":
                    return ProxyServerStatus.Open;
                // Server is completely down
                case "DOWN":
                    return ProxyServerStatus.Down;
                case "MAINT":
                    return ProxyServerStatus.Maintenance;
                // Server is not checked
                case "no check":
                    return ProxyServerStatus.NotChecked;
                // We have no idea what happened to this poor server, someone go check on it?...or we're not a server
                default:
                    return ProxyServerStatus.None;
            }
        }
    }
}
