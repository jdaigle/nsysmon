using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSysmon.Core.WMI
{
    public class WindowsServerNode : PollNode
    {
        private static log4net.ILog Logger = log4net.LogManager.GetLogger(typeof(WindowsServerNode));

        public override string NodeType { get { return "Windows Server"; } }
        public override int MinSecondsBetweenPolls { get { return 5; } }

        private readonly WindowsServerNodeSettings settings;
        public string NodeName { get; private set; }

        public WindowsServerNode(string nodeName, WindowsServerNodeSettings settings)
            : base(nodeName)
        {
            this.NodeName = nodeName;
            this.settings = settings;
        }

        public override IEnumerable<DataPoller> DataPollers
        {
            get
            {
                yield return Win32ComputerSystem;
                yield return Win32Volumes;
                yield return PingPoller;
                yield return PerfOSProcessor;
            }
        }

        protected override IEnumerable<MonitorStatus> GetMonitorStatus()
        {
            yield return PingPoller.MonitorStatus;
        }

        protected override string GetMonitorStatusReason()
        {
            return null;
        }

        private DataPoller<List<PerfOSProcessor>> _perfOSProcessor;
        public DataPoller<List<PerfOSProcessor>> PerfOSProcessor
        {
            get
            {
                return _perfOSProcessor ?? (_perfOSProcessor = new DataPoller<List<PerfOSProcessor>>()
                {
                    UpdateCachedData = UpdateDataPollerCachedData(
                        description: string.Format("WMI Query Win32_PerfFormattedData_PerfOS_Processor On Computer {0} ", settings.Host),
                        getData: () => Instrumentation.Query(settings.Host, settings.WMIPollingSettings,
                            "select Name, PercentProcessorTime from Win32_PerfFormattedData_PerfOS_Processor",
                            results => results.Select(mo => new PerfOSProcessor()
                            {
                                Name = mo["Name"].ToString() == "_Total" ? "Total" : mo["Name"].ToString(),
                                Utilization = (UInt64)mo["PercentProcessorTime"],
                            })).Data.ToList()
                    ),
                });
            }
        }

        private DataPoller<List<Win32Volume>> _win32Volumes;
        public DataPoller<List<Win32Volume>> Win32Volumes
        {
            get
            {
                return _win32Volumes ?? (_win32Volumes = new DataPoller<List<Win32Volume>>()
                {
                    CacheForSeconds = 60 * 15, // 15 minutes
                    UpdateCachedData = UpdateDataPollerCachedData(
                        description: string.Format("WMI Query Win32_Volume On Computer {0} ", settings.Host),
                        getData: () => Instrumentation.Query(settings.Host, settings.WMIPollingSettings,
                            "select name, label, drivetype, driveletter, capacity, freespace, filesystem from Win32_Volume",
                            results => results.Select(mo => new Win32Volume()
                            {
                                Name = (mo["name"] ?? string.Empty).ToString(),
                                Label = (mo["label"] ?? string.Empty).ToString(),
                                DriveType = (UInt32)(mo["drivetype"] ?? (UInt32)0),
                                DriveLetter = (mo["driveletter"] ?? string.Empty).ToString(),
                                FileSystem = (mo["filesystem"] ?? string.Empty).ToString(),
                                Capacity = (UInt64)(mo["capacity"] ?? (UInt64)0),
                                FreeSpace = (UInt64)(mo["freespace"] ?? (UInt64)0), 
                            })).Data.ToList()
                        ),
                });
            }
        }

        private DataPoller<Win32ComputerSystem> _win32ComputerSystem;
        public DataPoller<Win32ComputerSystem> Win32ComputerSystem
        {
            get
            {
                return _win32ComputerSystem ?? (_win32ComputerSystem = new DataPoller<Win32ComputerSystem>()
                {
                    CacheForSeconds = 60 * 60, // 1 hour
                    UpdateCachedData = UpdateDataPollerCachedData(
                        description: string.Format("WMI Query Win32_ComputerSystem/Win32_OperatingSystem On Computer {0} ", settings.Host),
                        getData: () =>
                            {
                                var Win32_ComputerSystem = Instrumentation.Query<dynamic>(settings.Host, settings.WMIPollingSettings,
                                    "select name, domain, manufacturer, model, totalphysicalmemory from Win32_ComputerSystem",
                                    results => results.Select(mo => new
                                    {
                                        Name = mo["name"].ToString(),
                                        Domain = mo["domain"].ToString(),
                                        Manufacturer = mo["manufacturer"].ToString(),
                                        Model = mo["model"].ToString(),
                                        TotalPhysicalMemory = (UInt64)mo["totalphysicalmemory"],
                                    }).Cast<dynamic>());

                                var Win32_OperatingSystem = Instrumentation.Query<dynamic>(settings.Host, settings.WMIPollingSettings,
                                    "select version, buildnumber from Win32_OperatingSystem",
                                    results => results.Select(mo => new
                                    {
                                        Version = mo["version"].ToString(),
                                        BuildNumber = mo["buildnumber"].ToString(),
                                    }).Cast<dynamic>());

                                return new Win32ComputerSystem()
                                {
                                    Name = Win32_ComputerSystem.Data.Single().Name,
                                    Domain = Win32_ComputerSystem.Data.Single().Domain,
                                    Manufacturer = Win32_ComputerSystem.Data.Single().Manufacturer,
                                    Model = Win32_ComputerSystem.Data.Single().Model,
                                    TotalPhysicalMemory = Win32_ComputerSystem.Data.Single().TotalPhysicalMemory,
                                    OSVersion = Win32_OperatingSystem.Data.Single().Version,
                                    OSBuildNumber = Win32_OperatingSystem.Data.Single().BuildNumber,
                                };
                            }
                        ),
                });
            }
        }

        private DataPoller<System.Net.NetworkInformation.PingReply> _pingPoller;
        private System.Net.NetworkInformation.Ping pinger = new System.Net.NetworkInformation.Ping();
        public DataPoller<System.Net.NetworkInformation.PingReply> PingPoller
        {
            get
            {
                return _pingPoller ?? (_pingPoller = new DataPoller<System.Net.NetworkInformation.PingReply>()
                {
                    //CacheForSeconds = 10,
                    UpdateCachedData = UpdateDataPollerCachedData(
                        description: string.Format("Ping host {0} ", settings.Host),
                        getData: () =>
                            {
                                return pinger.Send(settings.Host);
                            }
                    ),
                });
            }
        }
    }
}
