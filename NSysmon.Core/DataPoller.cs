using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NSysmon.Core
{
    /// <remarks>
    /// This code is derived from https://github.com/opserver/Opserver/tree/a170ea8bcda9f9e52d4aaff7339f3d198309369b
    /// under "The MIT License (MIT)". Copyright (c) 2013 Stack Exchange Inc.
    /// </remarks>
    public abstract class DataPoller : IMonitorStatus
    {
        /// <summary>
        /// Info for monitoring the monitoring, debugging, etc.
        /// </summary>
        public string ParentMemberName { get; protected set; }
        public string SourceFilePath { get; protected set; }
        public int SourceLineNumber { get; protected set; }
        protected DataPoller([CallerMemberName] string memberName = "",
                             [CallerFilePath] string sourceFilePath = "",
                             [CallerLineNumber] int sourceLineNumber = 0)
        {
            UniqueId = Guid.NewGuid();
            ParentMemberName = memberName;
            SourceFilePath = sourceFilePath;
            SourceLineNumber = sourceLineNumber;
        }

        public Guid UniqueId { get; private set; }

        public int? CacheFailureForSeconds { get; set; }
        public int CacheForSeconds { get; set; }

        protected long _pollsTotal;
        protected long _pollsSuccessful;
        public long PollsTotal { get { return _pollsTotal; } }
        public long PollsSuccessful { get { return _pollsSuccessful; } }
        public DateTime LastPoll { get; internal set; }
        public DateTime NextPoll
        {
            get
            {
                return LastPoll.AddSeconds(LastPollStatus == PollStatus.Fail
                                               ? CacheFailureForSeconds.GetValueOrDefault(CacheForSeconds)
                                               : CacheForSeconds);
            }
        }
        public bool IsStale { get { return NextPoll < DateTime.UtcNow; } }
        internal bool NeedsPoll = true;
        private volatile bool _isPolling;
        public bool IsPolling { get { return _isPolling; } internal set { _isPolling = value; } }

        public TimeSpan LastPollDuration { get; internal set; }
        public DateTime? LastSuccess { get; internal set; }
        public PollStatus LastPollStatus { get; set; }
        public string ErrorMessage { get; internal set; }

        public bool AffectsNodeStatus { get; set; }
        public MonitorStatus MonitorStatus
        {
            get
            {
                if (LastPoll == DateTime.MinValue) return MonitorStatus.Unknown;
                return LastPollStatus == PollStatus.Fail ? MonitorStatus.Critical : MonitorStatus.Good;
            }
        }

        public string MonitorStatusReason
        {
            get
            {
                if (LastPoll == DateTime.MinValue) return "Never Polled";
                return LastPollStatus == PollStatus.Fail ? "Poll " + LastPoll.ToRelativeTime() + " failed: " + ErrorMessage : null;
            }
        }

        public abstract bool ContainsCachedData { get; }
        public abstract object CachedData { get; }
        public virtual Type Type { get { return typeof(DataPoller); } }

        public abstract int Poll(bool force = false);
    }

    /// <remarks>
    /// This code is derived from https://github.com/opserver/Opserver/tree/a170ea8bcda9f9e52d4aaff7339f3d198309369b
    /// under "The MIT License (MIT)". Copyright (c) 2013 Stack Exchange Inc.
    /// </remarks>
    public class DataPoller<T> : DataPoller where T : class
    {
        public override bool ContainsCachedData { get { return cachedData != null; } }
        public override object CachedData { get { return cachedData; } }
        public override Type Type { get { return typeof(T); } }

        private T cachedData;
        public T Data
        {
            get
            {
                if (NeedsPoll)
                {
                    Poll();
                }
                return cachedData;
            }
            internal set { cachedData = value; }
        }

        /// <summary>
        /// Action to call to update the cached data during a Poll loop.
        /// </summary>
        public Action<DataPoller<T>> UpdateCachedData { get; set; }

        public override int Poll(bool force = false)
        {
            if (force)
            {
                NeedsPoll = true;
            }

            if (!NeedsPoll && !IsStale)
            {
                return 0;
            }

            if (IsPolling) return 0;

            var sw = Stopwatch.StartNew();
            IsPolling = true;
            try
            {
                Interlocked.Increment(ref _pollsTotal);
                if (UpdateCachedData != null)
                {
                    UpdateCachedData(this);
                }
                LastPollStatus = LastSuccess.HasValue && LastSuccess == LastPoll
                                     ? PollStatus.Success
                                     : PollStatus.Fail;
                NeedsPoll = false;
                if (cachedData != null)
                    Interlocked.Increment(ref _pollsSuccessful);
                return cachedData != null ? 1 : 0;
            }
            catch (Exception e)
            {
                ErrorMessage = e.Message;
                if (e.InnerException != null)
                {
                    ErrorMessage += "\n" + e.InnerException.Message;
                }
                LastPollStatus = PollStatus.Fail;
                return 0;
            }
            finally
            {
                IsPolling = false;
                sw.Stop();
                LastPollDuration = sw.Elapsed;
            }
        }
    }
}
