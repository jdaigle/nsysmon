using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSysmon.Core.Api
{
    public class NodeStatusViewModel : RazorViewModelBase
    {
        public string NodeType { get; set; }
        public string UniqueKey { get; set; }
        public DateTime? LastPoll { get; set; }
        public TimeSpan LastPollDuration { get; set; }
        public int MinSecondsBetweenPolls { get; set; }
        public MonitorStatus MonitorStatus { get; set; }
        public string MonitorStatusReason { get; set; }
        public string PollTaskStatus { get; set; }
        public int PollerCount { get; set; }

        public List<NodeDataCacheViewModel> DataCaches { get; set; }
    }

    public class NodeDataCacheViewModel : RazorViewModelBase
    {
        public string Name { get; set; }
        public Type Type { get; set; }
        public Guid UniqueId { get; set; }
        public DateTime LastPoll { get; set; }
        public TimeSpan LastPollDuration { get; set; }
        public PollStatus LastPollStatus { get; set; }
        public DateTime? LastSuccess { get; set; }
        public MonitorStatus MonitorStatus { get; set; }
        public string MonitorStatusReason { get; set; }
        public DateTime NextPoll { get; set; }
        public long PollsSuccessful { get; set; }
        public long PollsTotal { get; set; }
        public int? CacheFailureForSeconds { get; set; }
        public int CacheForSecond { get; set; }
        public int CachedDataCount { get; set; }
        public List<IDictionary<string, object>> CachedData { get; set; }
    }
}
