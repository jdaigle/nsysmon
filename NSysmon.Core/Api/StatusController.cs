using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace NSysmon.Core.Api
{
    public class StatusController : ApiController
    {
        [Route("api/status")]
        public object GetStatus()
        {
            return PollingEngine.AllPollNodes.Select(node => new
            {
                node.NodeType,
                node.UniqueKey,
                node.LastPoll,
                node.LastPollDuration,
                node.MinSecondsBetweenPolls,
                node.MonitorStatus,
                node.MonitorStatusReason,
                node.PollTaskStatus,
                DataCaches = node.DataCaches.Select(dataCache => new
                {
                    dataCache.Type,
                    dataCache.UniqueId,
                    dataCache.LastPoll,
                    dataCache.LastPollDuration,
                    dataCache.LastPollStatus,
                    dataCache.LastSuccess,
                    dataCache.MonitorStatus,
                    dataCache.MonitorStatusReason,
                    dataCache.NextPoll,
                    dataCache.PollsSuccessful,
                    dataCache.PollsTotal,
                    dataCache.CacheFailureForSeconds,
                    dataCache.CacheForSeconds,
                    CachedTrendData = dataCache.CachedTrendData.Select(d => new
                    {
                        DateTime = d.Item1,
                        Data = d.Item2,
                    }).ToList(),
                }).ToList(),
            }).ToList();
        }
    }
}
