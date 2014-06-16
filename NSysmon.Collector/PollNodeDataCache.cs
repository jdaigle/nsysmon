using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NSysmon.Collector
{
    /// <remarks>
    /// This code is derived from https://github.com/opserver/Opserver/tree/a170ea8bcda9f9e52d4aaff7339f3d198309369b
    /// under "The MIT License (MIT)". Copyright (c) 2013 Stack Exchange Inc.
    /// </remarks>
    public abstract class PollNodeDataCache
    {
        /// <summary>
        /// Info for monitoring the monitoring, debugging, etc.
        /// </summary>
        public string ParentMemberName { get; protected set; }
        public string SourceFilePath { get; protected set; }
        public int SourceLineNumber { get; protected set; }
        protected PollNodeDataCache(PollNode pollNode,
                                    int cacheForSeconds = 0,
                                    int? cacheFailureForSeconds = null,
                                    string description = "",
                                    [CallerMemberName] string memberName = "",
                                    [CallerFilePath] string sourceFilePath = "",
                                    [CallerLineNumber] int sourceLineNumber = 0)
        {
            this.PollNode = pollNode;
            this.UniqueId = Guid.NewGuid();
            this.CacheForSeconds = cacheForSeconds;
            this.CacheFailureForSeconds = cacheFailureForSeconds;
            this.Description = description;
            this.ParentMemberName = memberName;
            this.SourceFilePath = sourceFilePath;
            this.SourceLineNumber = sourceLineNumber;
        }

        public Guid UniqueId { get; private set; }
        public PollNode PollNode { get; private set; }
        public string Description { get; set; }

        /// <summary>
        /// The number of seconds to cache when LastRefreshStatus = failed
        /// </summary>
        public int? CacheFailureForSeconds { get; set; }
        /// <summary>
        /// The number of seconds for which the data will be cached.
        /// </summary>
        public int CacheForSeconds { get; set; }

        protected long refreshCountTotal;
        protected long refreshCountSuccessful;
        public long RefreshCountTotal { get { return refreshCountTotal; } }
        public long RefreshCountSuccessful { get { return refreshCountSuccessful; } }
        /// <summary>
        /// The DateTime after which this cache is considered stale
        /// </summary>
        public DateTime CacheExpiration
        {
            get
            {
                return LastRefresh.AddSeconds(LastRefreshStatus == PollStatus.Fail
                                               ? CacheFailureForSeconds.GetValueOrDefault(CacheForSeconds)
                                               : CacheForSeconds);
            }
        }
        public bool IsStale { get { return CacheExpiration < DateTime.UtcNow; } }
        protected volatile bool isPolling;
        public bool IsPolling { get { return isPolling; } }

        /// <summary>
        /// The DateTime at which we last attempted to refresh
        /// </summary>
        public DateTime LastRefresh { get; protected set; }
        /// <summary>
        /// The TimeSpan it took to last refresh the data.
        /// </summary>
        public TimeSpan LastRefreshDuration { get; internal set; }
        /// <summary>
        /// The DateTime at which we last successfully refreshed the data in this cache
        /// </summary>
        public DateTime? LastRefreshSuccess { get; internal set; }
        public PollStatus LastRefreshStatus { get; set; }
        public Exception LastRefreshError { get; internal set; }

        public abstract bool ContainsCachedData();
        public abstract object CachedData { get; }
        public abstract Type CachedDataType { get; }

        /// <summary>
        /// Refreshes the cache and returns the number of cached objects.
        /// </summary>
        public abstract int Refresh(bool force = false);
    }

    /// <remarks>
    /// This code is derived from https://github.com/opserver/Opserver/tree/a170ea8bcda9f9e52d4aaff7339f3d198309369b
    /// under "The MIT License (MIT)". Copyright (c) 2013 Stack Exchange Inc.
    /// </remarks>
    public class PollNodeDataCache<T> : PollNodeDataCache where T : class
    {
        public PollNodeDataCache(PollNode pollNode,
                                 Func<T> getData,
                                 int cacheForSeconds = 0,
                                 int? cacheFailureForSeconds = null,
                                 string description = "",
                                 [CallerMemberName] string memberName = "",
                                 [CallerFilePath] string sourceFilePath = "",
                                 [CallerLineNumber] int sourceLineNumber = 0)
            : base(pollNode, cacheForSeconds, cacheFailureForSeconds, description, memberName, sourceFilePath, sourceLineNumber)
        {
            this.GetData = getData;
        }

        public override bool ContainsCachedData() { return cachedData != null; }
        public override object CachedData { get { return cachedData; } }
        public override Type CachedDataType { get { return typeof(T); } }

        private T cachedData;
        public T Data
        {
            get
            {
                if (!ContainsCachedData())
                {
                    Refresh();
                }
                return cachedData;
            }
        }

        /// <summary>
        /// Action to call to update the cached data during a Poll loop.
        /// </summary>
        public Func<T> GetData { get; set; }

        public override int Refresh(bool force = false)
        {
            if (ContainsCachedData() && !force && !IsStale)
            {
                return 0;
            }

            if (IsPolling)
            {
                return 0;
            }

            var sw = Stopwatch.StartNew();
            this.isPolling = true;
            try
            {
                LastRefresh = DateTime.UtcNow;
                Interlocked.Increment(ref refreshCountTotal);

                this.cachedData = GetData();

                LastRefreshSuccess = LastRefresh;
                LastRefreshError = null;
                if (ContainsCachedData())
                {
                    Interlocked.Increment(ref refreshCountSuccessful);
                    LastRefreshStatus = PollStatus.Success;
                    return 1;
                }
                else
                {
                    return 0;
                }
            }
            catch (Exception e)
            {
                // capture the exception, and then continue
                // passing the exception up the callstack
                LastRefreshError = e;
                LastRefreshStatus = PollStatus.Fail;
                throw; // the caller will handle the exception
            }
            finally
            {
                isPolling = false;
                sw.Stop();
                LastRefreshDuration = sw.Elapsed;
            }
        }
    }
}
