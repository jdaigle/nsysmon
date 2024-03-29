﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSysmon.Collector
{
    /// <remarks>
    /// This code is derived from https://github.com/opserver/Opserver/tree/a170ea8bcda9f9e52d4aaff7339f3d198309369b
    /// under "The MIT License (MIT)". Copyright (c) 2013 Stack Exchange Inc.
    /// </remarks>
    public static class DateTimeExtensions
    {
        /// <summary>
        /// Converts to Date given an Epoch time
        /// </summary>
        public static DateTime ToDateTime(this long epoch)
        {
            return new DateTime(1970, 1, 1, 0, 0, 0).AddSeconds(epoch);
        }

        /// <summary>
        /// Returns a humanized string indicating how long ago something happened, eg "3 days ago".
        /// For future dates, returns when this DateTime will occur from DateTime.UtcNow.
        /// </summary>
        public static string ToRelativeTime(this DateTime dt, bool includeTime = true, bool asPlusMinus = false, DateTime? compareTo = null, bool includeSign = true)
        {
            var comp = (compareTo ?? DateTime.UtcNow);
            if (asPlusMinus)
            {
                return dt <= comp ? ToRelativeTimePastSimple(dt, comp, includeSign) : ToRelativeTimeFutureSimple(dt, comp, includeSign);
            }
            return dt <= comp ? ToRelativeTimePast(dt, comp, includeTime) : ToRelativeTimeFuture(dt, comp, includeTime);
        }
        /// <summary>
        /// Returns a humanized string indicating how long ago something happened, eg "3 days ago".
        /// For future dates, returns when this DateTime will occur from DateTime.UtcNow.
        /// If this DateTime is null, returns empty string.
        /// </summary>
        public static string ToRelativeTime(this DateTime? dt, bool includeTime = true)
        {
            if (dt == null) return "";
            return ToRelativeTime(dt.Value, includeTime);
        }

        private static string ToRelativeTimePast(DateTime dt, DateTime utcNow, bool includeTime = true)
        {
            TimeSpan ts = utcNow - dt;
            double delta = ts.TotalSeconds;

            if (delta < 1)
            {
                return "just now";
            }
            if (delta < 60)
            {
                return ts.Seconds == 1 ? "1 sec ago" : ts.Seconds + " secs ago";
            }
            if (delta < 3600) // 60 mins * 60 sec
            {
                return ts.Minutes == 1 ? "1 min ago" : ts.Minutes + " mins ago";
            }
            if (delta < 86400)  // 24 hrs * 60 mins * 60 sec
            {
                return ts.Hours == 1 ? "1 hour ago" : ts.Hours + " hours ago";
            }

            var days = ts.Days;
            if (days == 1)
            {
                return "yesterday";
            }
            if (days <= 2)
            {
                return days + " days ago";
            }
            if (utcNow.Year == dt.Year)
            {
                return dt.ToString(includeTime ? "MMM %d 'at' %H:mmm" : "MMM %d");
            }
            return dt.ToString(includeTime ? @"MMM %d \'yy 'at' %H:mmm" : @"MMM %d \'yy");
        }

        private static string ToRelativeTimeFuture(DateTime dt, DateTime utcNow, bool includeTime = true)
        {
            TimeSpan ts = dt - utcNow;
            double delta = ts.TotalSeconds;

            if (delta < 1)
            {
                return "just now";
            }
            if (delta < 60)
            {
                return ts.Seconds == 1 ? "in 1 second" : "in " + ts.Seconds + " seconds";
            }
            if (delta < 3600) // 60 mins * 60 sec
            {
                return ts.Minutes == 1 ? "in 1 minute" : "in " + ts.Minutes + " minutes";
            }
            if (delta < 86400) // 24 hrs * 60 mins * 60 sec
            {
                return ts.Hours == 1 ? "in 1 hour" : "in " + ts.Hours + " hours";
            }

            // use our own rounding so we can round the correct direction for future
            var days = (int)Math.Round(ts.TotalDays, 0);
            if (days == 1)
            {
                return "tomorrow";
            }
            if (days <= 10)
            {
                return "in " + days + " day" + (days > 1 ? "s" : "");
            }
            // if the date is in the future enough to be in a different year, display the year
            if (utcNow.Year == dt.Year)
            {
                return "on " + dt.ToString(includeTime ? "MMM %d 'at' %H:mmm" : "MMM %d");
            }
            return "on " + dt.ToString(includeTime ? @"MMM %d \'yy 'at' %H:mmm" : @"MMM %d \'yy");
        }

        private static string ToRelativeTimePastSimple(DateTime dt, DateTime utcNow, bool includeSign)
        {
            TimeSpan ts = utcNow - dt;
            var sign = includeSign ? "-" : "";
            double delta = ts.TotalSeconds;
            if (delta < 1)
                return "< 1 sec";
            if (delta < 60)
                return sign + ts.Seconds + " sec" + (ts.Seconds == 1 ? "" : "s");
            if (delta < 3600) // 60 mins * 60 sec
                return sign + ts.Minutes + " min" + (ts.Minutes == 1 ? "" : "s");
            if (delta < 86400) // 24 hrs * 60 mins * 60 sec
                return sign + ts.Hours + " hour" + (ts.Hours == 1 ? "" : "s");
            return sign + ts.Days + " days";
        }

        private static string ToRelativeTimeFutureSimple(DateTime dt, DateTime utcNow, bool includeSign)
        {
            TimeSpan ts = dt - utcNow;
            double delta = ts.TotalSeconds;
            var sign = includeSign ? "+" : "";

            if (delta < 1)
                return "< 1 sec";
            if (delta < 60)
                return sign + ts.Seconds + " sec" + (ts.Seconds == 1 ? "" : "s");
            if (delta < 3600) // 60 mins * 60 sec
                return sign + ts.Minutes + " min" + (ts.Minutes == 1 ? "" : "s");
            if (delta < 86400) // 24 hrs * 60 mins * 60 sec
                return sign + ts.Hours + " hour" + (ts.Hours == 1 ? "" : "s");
            return sign + ts.Days + " days";
        }
    }
}
