using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NSysmon.Core
{
    /// <remarks>
    /// This code is derived from https://github.com/opserver/Opserver/tree/a170ea8bcda9f9e52d4aaff7339f3d198309369b
    /// under "The MIT License (MIT)". Copyright (c) 2013 Stack Exchange Inc.
    /// </remarks>
    public abstract class PollNode : IMonitorStatus, IDisposable, IEquatable<PollNode>
    {
        private static log4net.ILog Logger = log4net.LogManager.GetLogger(typeof(PollNode));

        private int _totalPolls;
        private int _totalCachePolls;

        public abstract int MinSecondsBetweenPolls { get; }
        public abstract string NodeType { get; }
        public abstract IEnumerable<PollNodeDataCache> DataCaches { get; }
        protected abstract IEnumerable<MonitorStatus> GetMonitorStatus();
        protected abstract string GetMonitorStatusReason();

        /// <summary>
        /// Number of consecutive cache fetch failures before backing off of polling the entire node for <see cref="BackoffDuration"/>
        /// </summary>
        protected virtual int FailsBeforeBackoff { get { return 3; } }
        /// <summary>
        /// Length of time to backoff once <see cref="FailsBeforeBackoff"/> is hit
        /// </summary>
        protected virtual TimeSpan BackoffDuration { get { return TimeSpan.FromMinutes(2); } }

        public string UniqueKey { get; private set; }

        protected PollNode(string uniqueKey)
        {
            UniqueKey = uniqueKey;
        }

        public DateTime? LastPoll { get; protected set; }
        public TimeSpan LastPollDuration { get; protected set; }
        protected int PollFailsInaRow = 0;

        protected volatile bool _isPolling;
        public bool IsPolling { get { return _isPolling; } }
        protected Task _pollTask;
        public virtual string PollTaskStatus
        {
            get { return _pollTask != null ? _pollTask.Status.ToString() : "Not running"; }
        }

        private readonly object _monitorStatusLock = new object();
        protected MonitorStatus? CachedMonitorStatus;
        public virtual MonitorStatus MonitorStatus
        {
            get
            {
                if (!CachedMonitorStatus.HasValue)
                {
                    lock (_monitorStatusLock)
                    {
                        if (CachedMonitorStatus.HasValue)
                        {
                            return CachedMonitorStatus.Value;
                        }

                        var pollers = DataCaches.Where(dp => dp.AffectsNodeStatus).ToList();
                        var fetchStatus = pollers.GetWorstStatus();
                        if (fetchStatus != MonitorStatus.Good)
                        {
                            CachedMonitorStatus = MonitorStatus.Critical;
                            MonitorStatusReason =
                                string.Join(", ", pollers.WithIssues()
                                                         .GroupBy(g => g.MonitorStatus)
                                                         .OrderByDescending(g => g.Key)
                                                         .Select(
                                                             g =>
                                                             g.Key + ": " + string.Join(", ", g.Select(p => p.ParentMemberName))
                                                      ));
                        }
                        else
                        {
                            CachedMonitorStatus = GetMonitorStatus().ToList().GetWorstStatus();
                            MonitorStatusReason = CachedMonitorStatus == MonitorStatus.Good ? null : GetMonitorStatusReason();
                        }
                    }
                }
                return CachedMonitorStatus.GetValueOrDefault(MonitorStatus.Unknown);
            }
        }
        public string MonitorStatusReason { get; private set; }

        public virtual void Poll(bool force = false, bool sync = false)
        {
            // Don't poll more than once every n seconds, that's just rude
            if (!force && DateTime.UtcNow < LastPoll.GetValueOrDefault().AddSeconds(MinSecondsBetweenPolls))
                return;

            // If we're seeing a lot of poll failures in a row, back the hell off
            if (!force && PollFailsInaRow >= FailsBeforeBackoff && DateTime.UtcNow < LastPoll.GetValueOrDefault() + BackoffDuration)
                return;

            // Prevent multiple poll threads for this node from running at once
            if (_isPolling) return;
            _isPolling = true;

            if (sync)
                InnerPoll(force);
            else
                _pollTask = Task.Factory.StartNew(() => InnerPoll(force));
        }

        /// <summary>
        /// Called on a background thread for when this node is ACTUALLY polling
        /// This is not called if we're not due for a poll when the pass runs
        /// </summary>
        private void InnerPoll(bool force = false)
        {
            Logger.DebugFormat("Starting Poll of Node [{0} - {1}]", this.NodeType, this.UniqueKey);
            var sw = Stopwatch.StartNew();
            try
            {
                var polled = 0;
                Parallel.ForEach(DataCaches, i =>
                {
                    var pollerResult = i.Poll(force);
                    Interlocked.Add(ref polled, pollerResult);
                });
                LastPoll = DateTime.UtcNow;

                Interlocked.Add(ref _totalCachePolls, polled);
                Interlocked.Increment(ref _totalPolls);
            }
            finally
            {
                sw.Stop();
                LastPollDuration = sw.Elapsed;
                _isPolling = false;
                _pollTask = null;
                Logger.DebugFormat("End Poll of Node [{0} - {1}] in {2} ms.", this.NodeType, this.UniqueKey, LastPollDuration.TotalMilliseconds.ToString("N2"));
            }
        }

        /// <summary>
        /// Invoked by a Cache instance on updating, using properties from the PollNode such as connection strings, etc.
        /// </summary>
        /// <typeparam name="T">Type of item in the cache</typeparam>
        /// <param name="description">Description of the operation, used purely for profiling</param>
        /// <param name="getData">The operation used to actually get data, e.g. <code>using (var conn = GetConnection()) { return getFromConnection(conn); }</code></param>
        /// <param name="logExceptions">Whether to log any exceptions to the log</param>
        /// <returns>A cache update action, used when creating a <see cref="Cache"/>.</returns>
        /// <param name="addExceptionData">Optionally add exception data, e.g. <code>e => e.AddLoggedData("Server", Name)</code></param>
        protected Action<PollNodeDataCode<T>> UpdateCachedData<T>(string description,
                                                            Func<T> getData,
                                                            bool logExceptions = false,
                                                            Action<Exception> addExceptionData = null) where T : class
        {
            return cache =>
            {
                try
                {
                    Logger.DebugFormat("Getting Poller Data [{0}]", description);
                    cache.Data = getData();
                    cache.LastSuccess = cache.LastPoll = DateTime.UtcNow;
                    cache.ErrorMessage = "";
                    PollFailsInaRow = 0;
                }
                catch (Exception e)
                {
                    if (logExceptions)
                    {
                        if (addExceptionData != null)
                        {
                            addExceptionData(e);
                        }
                        ExceptionManager.LogException(e);
                    }
                    cache.LastPoll = DateTime.UtcNow;
                    PollFailsInaRow++;
                    cache.ErrorMessage = "Unable to fetch from " + NodeType + ": " + e.Message;
                    if (e.InnerException != null)
                    {
                        cache.ErrorMessage += "\n" + e.InnerException.Message;
                    }
                    Logger.Error(cache.ErrorMessage, e);
                }
                CachedMonitorStatus = null;
            };
        }

        public bool Equals(PollNode other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return other.GetType() == this.GetType() && string.Equals(this.UniqueKey, other.UniqueKey);
        }

        public override int GetHashCode()
        {
            return (UniqueKey != null ? UniqueKey.GetHashCode() : 0);
        }

        public static bool operator ==(PollNode left, PollNode right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(PollNode left, PollNode right)
        {
            return !Equals(left, right);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((PollNode)obj);
        }

        public void Dispose()
        {
            PollingEngine.TryRemove(this);
        }
    }
}
