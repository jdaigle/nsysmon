using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSysmon.Collector.WMI
{
    /// <remarks>
    /// This code is derived from https://github.com/opserver/Opserver/tree/a170ea8bcda9f9e52d4aaff7339f3d198309369b
    /// under "The MIT License (MIT)". Copyright (c) 2013 Stack Exchange Inc.
    /// </remarks>
    public class WMIPollingSettings
    {
        private const int DefaultTime = 30 * 1000;

        public WMIPollingSettings()
        {
            QueryTimeout = DefaultTime;
        }

        public WMIPollingSettings(string authUser, string authPassword)
            :this()
        {
            this.AuthUser = authUser;
            this.AuthPassword = authPassword;
        }

        /// <summary>
        /// Maximum timeout in milliseconds before giving up on a poll
        /// </summary>
        public int QueryTimeout { get; set; }

        /// <summary>
        /// User to authenticate as, if not present then impersonation will be used
        /// </summary>
        public string AuthUser { get; set; }

        /// <summary>
        /// Password for user to authenticate as, if not present then impersonation will be used
        /// </summary>
        public string AuthPassword { get; set; }
    }
}
