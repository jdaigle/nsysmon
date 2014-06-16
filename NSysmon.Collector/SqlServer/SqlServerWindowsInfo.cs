using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <remarks>
/// This code is derived from https://github.com/opserver/Opserver/tree/a170ea8bcda9f9e52d4aaff7339f3d198309369b
/// under "The MIT License (MIT)". Copyright (c) 2013 Stack Exchange Inc.
/// </remarks>
namespace NSysmon.Collector.SqlServer
{
    public class SqlServerWindowsInfo
    {
        public string WindowsRelease { get; private set; }
        public string WindowsServicePackLevel { get; private set; }
        /// <summary>
        /// see GetProductInfo() http://msdn.microsoft.com/en-us/library/ms724358.aspx
        /// </summary>
        public int WindowsSKU { get; private set; }
        public int OSLanguageVersion { get; private set; }

        public string WindowsReleaseName
        {
            get
            {
                switch (WindowsRelease)
                {
                    case "5.0": return "Windows 2000";
                    case "5.1": return "Windows XP";
                    case "5.2": return "Windows Server 2003";
                    case "6.0": return "Windows Server 2008";
                    case "6.1": return "Windows Server 2008 R2";
                    case "6.2": return "Windows Server 2012";
                    case "6.3": return "Windows Server 2012 R2";
                    default: return "Other";
                }
            }
        }

        internal const string FetchSQL = @"
SELECT
    windows_release AS WindowsRelease
    , windows_service_pack_level AS WindowsServicePackLevel
    , windows_sku AS WindowsSKU
    , os_language_version AS OSLanguageVersion
FROM sys.dm_os_windows_info;";
    }
}
