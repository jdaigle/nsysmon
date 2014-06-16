using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NSysmon.Collector
{
    /// <remarks>
    /// This code is derived from https://github.com/opserver/Opserver/tree/a170ea8bcda9f9e52d4aaff7339f3d198309369b
    /// under "The MIT License (MIT)". Copyright (c) 2013 Stack Exchange Inc.
    /// </remarks>
    public abstract class PollNode : IEquatable<PollNode>
    {
        /// <summary>
        /// A unique identifier for this PollNode
        /// </summary>
        public string UniqueKey { get; private set; }

        protected PollNode(string uniqueKey)
        {
            UniqueKey = uniqueKey;
        }

        private static log4net.ILog Logger = log4net.LogManager.GetLogger(typeof(PollNode));

        /// <summary>
        /// the number of times this node was polled since startup
        /// </summary>
        private int totalPolls;
        /// <summary>
        /// the number of times an inner poll was executed
        /// </summary>
        private int totalCachePolls;

        /// <summary>
        /// The minimum number of seconds we will wait before polling this node again
        /// </summary>
        public abstract int MinSecondsBetweenPolls { get; }
        /// <summary>
        /// Description of the concrete type of this PollNode
        /// </summary>
        public abstract string NodeType { get; }
        /// <summary>
        /// Enumerates the various data caches applicable to this poll node.
        /// Each data cache will be refreshed during a POLL interval
        /// </summary>
        public abstract IEnumerable<PollNodeDataCache> DataCaches { get; }

        /// <summary>
        /// Number of consecutive cache fetch failures before backing off of polling the entire node for <see cref="BackoffDuration"/>
        /// </summary>
        protected virtual int FailsBeforeBackoff { get { return 3; } }
        /// <summary>
        /// Length of time to backoff once <see cref="FailsBeforeBackoff"/> is hit
        /// </summary>
        protected virtual TimeSpan BackoffDuration { get { return TimeSpan.FromMinutes(2); } }

        public DateTime? LastPoll { get; protected set; }
        public TimeSpan LastPollDuration { get; protected set; }
        protected int PollFailsInaRow = 0;

        protected volatile bool isPolling;
        public bool IsPolling { get { return isPolling; } }
        protected Task pollTask;
        public virtual string PollTaskStatus
        {
            get { return this.pollTask != null ? this.pollTask.Status.ToString() : "Not running"; }
        }

        public virtual void Poll()
        {
            // Don't poll more than once every n seconds, that's just rude
            if (DateTime.UtcNow < LastPoll.GetValueOrDefault().AddSeconds(MinSecondsBetweenPolls))
            {
                return;
            }

            // If we're seeing a lot of poll failures in a row... then wait
            if (PollFailsInaRow >= FailsBeforeBackoff && DateTime.UtcNow < LastPoll.GetValueOrDefault() + BackoffDuration)
            {
                return;
            }

            // Prevent multiple poll threads for this node from running at once
            if (isPolling)
            {
                return;
            }

            isPolling = true;
            pollTask = Task.Factory.StartNew(() => InnerPoll());
        }

        /// <summary>
        /// Called on a background thread for when this node is ACTUALLY polling
        /// This is not called if we're not due for a poll when the pass runs
        /// </summary>
        private void InnerPoll()
        {
            Logger.DebugFormat("Starting Poll of Node [{0} - {1}]", this.NodeType, this.UniqueKey);
            var sw = Stopwatch.StartNew();
            try
            {
                var polled = 0;
                Parallel.ForEach(DataCaches, i =>
                {
                    try
                    {
                        var pollerResult = i.Refresh();
                        Interlocked.Add(ref polled, pollerResult);
                    }
                    catch (Exception e)
                    {
                        // handle the exception and move on
                        PollFailsInaRow++;
                        Logger.Error("Error During DataCache Refresh", e);
                    }
                });
                LastPoll = DateTime.UtcNow;
                Interlocked.Add(ref totalCachePolls, polled);
                Interlocked.Increment(ref totalPolls);
            }
            catch (Exception e)
            {
                PollFailsInaRow++;
                // the Refresh loop should not throw an exception, but
                // we handle them as Fatal exceptions anyway
                Logger.Fatal("Unexepcted exception in during Poll: ", e);
            }
            finally
            {
                sw.Stop();
                LastPollDuration = sw.Elapsed;
                isPolling = false;
                pollTask = null;
                Logger.DebugFormat("End Poll of Node [{0} - {1}] in {2} ms.", this.NodeType, this.UniqueKey, LastPollDuration.TotalMilliseconds.ToString("N2"));
            }
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

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((PollNode)obj);
        }
    }
}
