using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace NSysmon.Collector
{
    /// <remarks>
    /// This code is derived from https://github.com/opserver/Opserver/tree/a170ea8bcda9f9e52d4aaff7339f3d198309369b
    /// under "The MIT License (MIT)". Copyright (c) 2013 Stack Exchange Inc.
    /// </remarks>
    public class PollingEngine
    {
        protected static log4net.ILog Logger = log4net.LogManager.GetLogger(typeof(PollingEngine));

        private static readonly object _addLock = new object();

        public static HashSet<PollNode> AllPollNodes;
        private static Thread _globalPollingThread;
        private static volatile bool _shuttingDown;
        private static readonly object _pollAllLock;
        private static long _totalPollIntervals;
        private static DateTime? _lastPollAll;
        private static DateTime _startTime;

        static PollingEngine()
        {
            _pollAllLock = new object();
            AllPollNodes = new HashSet<PollNode>();
            StartPolling();
        }

        /// <summary>
        /// Adds a node to the global polling list ONLY IF IT IS NEW
        /// If a node with the same unique key was already added, it will not be added again
        /// </summary>
        /// <param name="node">The node to add to the global polling list</param>
        /// <returns>Whether the node was added</returns>
        public static bool TryAdd(PollNode node)
        {
            lock (_addLock)
            {
                var added = AllPollNodes.Add(node);
                if (added)
                {
                    Logger.InfoFormat("PollNode [{0} - {1}] added.", node.NodeType, node.UniqueKey);
                }
                return added;
            }
        }

        public static bool TryRemove(PollNode node)
        {
            if (node == null)
            {
                return false;
            }
            lock (_addLock)
            {
                return AllPollNodes.Remove(node);
            }
        }

        public static void StartPolling()
        {
            _startTime = DateTime.UtcNow;
            if (_globalPollingThread == null)
            {
                _globalPollingThread = new Thread(MonitorPollingLoop)
                {
                    Name = "GlobalPolling",
                    Priority = ThreadPriority.Lowest,
                    IsBackground = true
                };
            }
            if (!_globalPollingThread.IsAlive)
            {
                _globalPollingThread.Start();
                Logger.Info("Global Polling Thread Started");
            }
        }

        /// <summary>
        /// Performs a soft shutdown after the current poll finishes
        /// </summary>
        public static void StopPolling()
        {
            _shuttingDown = true;
            Logger.Info("Shutting Down Global Polling Thread");
        }

        private static void MonitorPollingLoop()
        {
            while (!_shuttingDown)
            {
                try
                {
                    while (!_shuttingDown)
                    {
                        PollAll();
                        Thread.Sleep(1000);
                    }
                }
                catch (ThreadAbortException e)
                {
                    if (!_shuttingDown)
                    {
                        ExceptionManager.LogException("Global polling loop shutting down", e);
                    }

                }
                catch (Exception ex)
                {
                    ExceptionManager.LogException(ex);
                }
                try
                {
                    Thread.Sleep(2000);
                }
                catch (ThreadAbortException)
                {
                    // application is cycling, AND THAT'S OKAY
                }
            }
        }

        public static void PollAll()
        {
            if (!Monitor.TryEnter(_pollAllLock, 500))
            {
                return;
            }

            Interlocked.Increment(ref _totalPollIntervals);
            try
            {
                Parallel.ForEach(AllPollNodes, i => i.Poll());
            }
            catch (Exception e)
            {
                ExceptionManager.LogException(e);
            }
            finally
            {
                Monitor.Exit(_pollAllLock);
            }
            _lastPollAll = DateTime.UtcNow;
        }

        public static List<PollNode> GetNodes(string type)
        {
            return AllPollNodes.Where(pn => string.Equals(pn.NodeType, type, StringComparison.InvariantCultureIgnoreCase)).ToList();
        }

        public static PollNode GetNode(string type, string key)
        {
            return AllPollNodes.FirstOrDefault(pn => string.Equals(pn.NodeType, type, StringComparison.InvariantCultureIgnoreCase) && pn.UniqueKey == key);
        }
    }
}
