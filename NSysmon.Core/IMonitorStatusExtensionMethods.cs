using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSysmon.Core
{
    /// <remarks>
    /// This code is derived from https://github.com/opserver/Opserver/tree/a170ea8bcda9f9e52d4aaff7339f3d198309369b
    /// under "The MIT License (MIT)". Copyright (c) 2013 Stack Exchange Inc.
    /// </remarks>
    public static class IMonitorStatusExtensionMethods
    {
        public static MonitorStatus GetWorstStatus(this IEnumerable<IMonitorStatus> ims, string cacheKey = null, int durationSeconds = 5)
        {
            MonitorStatus? result = null;
            //if (cacheKey.HasValue())
            //    result = Current.LocalCache.Get<MonitorStatus?>(cacheKey);
            if (result == null)
            {
                result = GetWorstStatus(ims.Select(i => i.MonitorStatus));
                //if (cacheKey.HasValue())
                //    Current.LocalCache.Set(cacheKey, result, durationSeconds);
            }
            return result.Value;
        }

        public static MonitorStatus GetWorstStatus(this IEnumerable<MonitorStatus> ims)
        {
            return ims.OrderByDescending(i => i).FirstOrDefault();
        }

        public static IEnumerable<T> WithIssues<T>(this IEnumerable<T> items) where T : IMonitorStatus
        {
            return items.Where(i => i.MonitorStatus != MonitorStatus.Good);
        }
    }
}
