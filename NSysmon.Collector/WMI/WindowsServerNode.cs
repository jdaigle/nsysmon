using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSysmon.Collector.WMI
{
    public class WindowsServerNode : PollNode
    {
        private static log4net.ILog Logger = log4net.LogManager.GetLogger(typeof(WindowsServerNode));

        public override string NodeType { get { return "Windows Server"; } }
        public override int MinSecondsBetweenPolls { get { return 10; } }

        private readonly WindowsServerNodeSettings settings;
        public string NodeName { get; private set; }

        public WindowsServerNode(string nodeName, WindowsServerNodeSettings settings)
            : base(nodeName)
        {
            this.NodeName = nodeName;
            this.settings = settings;
        }

        public override IEnumerable<PollNodeDataCache> DataCaches
        {
            get
            {
                yield return Win32ComputerSystem;
                yield return Win32Volumes;
                yield return Win32NetworkAdapters;
                yield return PingPoller;
                yield return PerfOSProcessor;
                yield return PerfOSMemory;
                yield return PerfOSPagingFiles;
                yield return PerfOSSystem;
                yield return PerfDiskPhysicalDisks;
                yield return TcpipNetworkInterfaces;
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

        public bool IsWindowsServer2008CompatOrBetter()
        {
            return char.IsNumber(TryGetOSVersion()[0]) && int.Parse(TryGetOSVersion()[0].ToString()) >= 6;
        }

        public string TryGetOSVersion()
        {
            if (_win32ComputerSystem != null
                && _win32ComputerSystem.ContainsCachedData() 
                && _win32ComputerSystem.Data != null)
            {
                return _win32ComputerSystem.Data.OSVersion;
            }
            return "Unknown";
        }

        private PollNodeDataCache<List<PerfOSProcessor>> _perfOSProcessor;
        public PollNodeDataCache<List<PerfOSProcessor>> PerfOSProcessor
        {
            get
            {
                return _perfOSProcessor ?? (_perfOSProcessor = new PollNodeDataCache<List<PerfOSProcessor>>()
                {
                    UpdateCachedData = UpdateCachedData(
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

        private PollNodeDataCache<PerfOSMemory> _perfOSMemory;
        public PollNodeDataCache<PerfOSMemory> PerfOSMemory
        {
            get
            {
                return _perfOSMemory ?? (_perfOSMemory = new PollNodeDataCache<PerfOSMemory>()
                {
                    UpdateCachedData = UpdateCachedData(
                        description: string.Format("WMI Query Win32_PerfFormattedData_PerfOS_Memory On Computer {0} ", settings.Host),
                        getData: () => Instrumentation.Query(settings.Host, settings.WMIPollingSettings,
                            "select AvailableBytes, AvailableMBytes from Win32_PerfFormattedData_PerfOS_Memory",
                            results => results.Select(mo => new PerfOSMemory()
                            {
                                AvailableBytes = (UInt64)mo["AvailableBytes"],
                                AvailableMBytes = (UInt64)mo["AvailableMBytes"],
                            })).Data.Single()
                    ),
                });
            }
        }

        private PollNodeDataCache<List<PerfDiskPhysicalDisk>> _perfDiskPhysicalDisk;
        public PollNodeDataCache<List<PerfDiskPhysicalDisk>> PerfDiskPhysicalDisks
        {
            get
            {
                return _perfDiskPhysicalDisk ?? (_perfDiskPhysicalDisk = new PollNodeDataCache<List<PerfDiskPhysicalDisk>>()
                {
                    UpdateCachedData = UpdateCachedData(
                        description: string.Format("WMI Query Win32_PerfFormattedData_PerfDisk_PhysicalDisk On Computer {0} ", settings.Host),
                        getData: () => Instrumentation.Query(settings.Host, settings.WMIPollingSettings,
                            "select AvgDiskSecPerRead, AvgDiskSecPerWrite, DiskReadsPerSec, DiskWritesPerSec, name from Win32_PerfFormattedData_PerfDisk_PhysicalDisk",
                            results => results.Select(mo => new PerfDiskPhysicalDisk()
                            {
                                Name = (mo["name"] ?? string.Empty).ToString(),
                                AvgDiskSecPerRead = (UInt32)mo["AvgDiskSecPerRead"],
                                AvgDiskSecPerWrite = (UInt32)mo["AvgDiskSecPerWrite"],
                                DiskReadsPerSec = (UInt32)mo["DiskReadsPerSec"],
                                DiskWritesPerSec = (UInt32)mo["DiskWritesPerSec"],
                            })).Data.ToList()
                    ),
                });
            }
        }

        private PollNodeDataCache<List<TcpipNetworkInterface>> _tcpipNetworkInterface;
        public PollNodeDataCache<List<TcpipNetworkInterface>> TcpipNetworkInterfaces
        {
            get
            {
                return _tcpipNetworkInterface ?? (_tcpipNetworkInterface = new PollNodeDataCache<List<TcpipNetworkInterface>>()
                {
                    UpdateCachedData = UpdateCachedData(
                        description: string.Format("WMI Query Win32_PerfFormattedData_Tcpip_NetworkInterface On Computer {0} ", settings.Host),
                        getData: () => 
                        {
                            if (!IsWindowsServer2008CompatOrBetter())
                            {
                                return new List<TcpipNetworkInterface>();
                            }
                            return
                                Instrumentation.Query(settings.Host, settings.WMIPollingSettings,
                                "select Name, Caption, Description, BytesReceivedPerSec, BytesSentPerSec, CurrentBandwidth from Win32_PerfFormattedData_Tcpip_NetworkInterface",
                                results => results.Select(mo => new TcpipNetworkInterface()
                                {
                                    Name = (mo["name"] ?? string.Empty).ToString(),
                                    Caption = (mo["Caption"] ?? string.Empty).ToString(),
                                    Description = (mo["Description"] ?? string.Empty).ToString(),
                                    BytesReceivedPerSec = (UInt64)mo["BytesReceivedPerSec"],
                                    BytesSentPerSec = (UInt64)mo["BytesSentPerSec"],
                                    CurrentBandwidth = (UInt64)mo["CurrentBandwidth"],
                                })).Data.ToList();
                        }
                    ),
                });
            }
        }

        private PollNodeDataCache<List<PerfOSPagingFile>> _perfOSPagingFiles;
        public PollNodeDataCache<List<PerfOSPagingFile>> PerfOSPagingFiles
        {
            get
            {
                return _perfOSPagingFiles ?? (_perfOSPagingFiles = new PollNodeDataCache<List<PerfOSPagingFile>>()
                {
                    UpdateCachedData = UpdateCachedData(
                        description: string.Format("WMI Query Win32_PerfFormattedData_PerfOS_PagingFile On Computer {0} ", settings.Host),
                        getData: () => Instrumentation.Query(settings.Host, settings.WMIPollingSettings,
                            "select PercentUsage, name from Win32_PerfFormattedData_PerfOS_PagingFile",
                            results => results.Select(mo => new PerfOSPagingFile()
                            {
                                Name = (mo["name"] ?? string.Empty).ToString(),
                                PercentUsage = (UInt32)mo["PercentUsage"],
                            })).Data.ToList()
                    ),
                });
            }
        }

        private PollNodeDataCache<PerfOSSystem> _perfOSSystem;
        public PollNodeDataCache<PerfOSSystem> PerfOSSystem
        {
            get
            {
                return _perfOSSystem ?? (_perfOSSystem = new PollNodeDataCache<PerfOSSystem>()
                {
                    UpdateCachedData = UpdateCachedData(
                        description: string.Format("WMI Query Win32_PerfFormattedData_PerfOS_System On Computer {0} ", settings.Host),
                        getData: () => Instrumentation.Query(settings.Host, settings.WMIPollingSettings,
                            "select ProcessorQueueLength, name from Win32_PerfFormattedData_PerfOS_System",
                            results => results.Select(mo => new PerfOSSystem()
                            {
                                Name = (mo["name"] ?? string.Empty).ToString(),
                                ProcessorQueueLength = (UInt32)mo["ProcessorQueueLength"],
                            })).Data.Single()
                    ),
                });
            }
        }

        private PollNodeDataCache<List<Win32Volume>> _win32Volumes;
        public PollNodeDataCache<List<Win32Volume>> Win32Volumes
        {
            get
            {
                return _win32Volumes ?? (_win32Volumes = new PollNodeDataCache<List<Win32Volume>>()
                {
                    CacheForSeconds = 60,
                    UpdateCachedData = UpdateCachedData(
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

        private PollNodeDataCache<Win32ComputerSystem> _win32ComputerSystem;
        public PollNodeDataCache<Win32ComputerSystem> Win32ComputerSystem
        {
            get
            {
                return _win32ComputerSystem ?? (_win32ComputerSystem = new PollNodeDataCache<Win32ComputerSystem>()
                {
                    CacheForSeconds = 60,
                    UpdateCachedData = UpdateCachedData(
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

        private PollNodeDataCache<List<Win32NetworkAdapter>> _win32NetworkAdapters;
        public PollNodeDataCache<List<Win32NetworkAdapter>> Win32NetworkAdapters
        {
            get
            {
                return _win32NetworkAdapters ?? (_win32NetworkAdapters = new PollNodeDataCache<List<Win32NetworkAdapter>>()
                {
                    CacheForSeconds = 60,
                    UpdateCachedData = UpdateCachedData(
                        description: string.Format("WMI Query Win32_NetworkAdapter On Computer {0} ", settings.Host),
                        getData: () =>
                            {
                                if (!IsWindowsServer2008CompatOrBetter())
                                {
                                    return new List<Win32NetworkAdapter>();
                                }
                                return
                                    Instrumentation.Query(settings.Host, settings.WMIPollingSettings,
                                    "select Name, Description, AdapterType, AdapterTypeId, MACAddress, NetConnectionID, NetConnectionStatus, NetEnabled, physicaladapter, ProductName, Manufacturer, TimeOfLastReset from Win32_NetworkAdapter",
                                    results => results.Select(mo => new Win32NetworkAdapter()
                                    {
                                        Name = (mo["Name"] ?? string.Empty).ToString(),
                                        Description = (mo["Description"] ?? string.Empty).ToString(),
                                        AdapterType = (mo["AdapterType"] ?? string.Empty).ToString(),
                                        AdapterTypeId = (UInt16)(mo["AdapterTypeId"] ?? (UInt16)0),
                                        MACAddress = (mo["MACAddress"] ?? string.Empty).ToString(),
                                        NetConnectionID = (mo["NetConnectionID"] ?? string.Empty).ToString(),
                                        NetConnectionStatus = (UInt16)(mo["NetConnectionStatus"] ?? (UInt16)0),
                                        NetEnabled = (bool)(mo["NetEnabled"] ?? false),
                                        PhysicalAdapter = (bool)mo["PhysicalAdapter"],
                                        ProductName = (mo["ProductName"] ?? string.Empty).ToString(),
                                        Manufacturer = (mo["Manufacturer"] ?? string.Empty).ToString(),
                                        TimeOfLastReset = (mo["TimeOfLastReset"] ?? string.Empty).ToString(),
                                        //TimeOfLastReset = DateTime.ParseExact(mo["TimeOfLastReset"].ToString(), "yyyyMMddHHmmss.ffffff-240", DateTimeFormatInfo.InvariantInfo),
                                    })).Data.ToList();
                            }
                        ),
                });
            }
        }

        private PollNodeDataCache<PingResult> _pingPoller;
        private System.Net.NetworkInformation.Ping pinger = new System.Net.NetworkInformation.Ping();
        public PollNodeDataCache<PingResult> PingPoller
        {
            get
            {
                return _pingPoller ?? (_pingPoller = new PollNodeDataCache<PingResult>()
                {
                    UpdateCachedData = UpdateCachedData(
                        description: string.Format("Ping host {0} ", settings.Host),
                        getData: () =>
                            {
                                var p = pinger.Send(settings.Host);
                                if (p == null)
                                {
                                    return new PingResult()
                                    {
                                        RoundtripTime = 0,
                                        Ttl = 0,
                                        BufferLength = 0,
                                        Status = "pinger.Send is null",
                                    };
                                }
                                return new PingResult
                                {
                                    RoundtripTime = p.RoundtripTime,
                                    Ttl = p.Options != null ? p.Options.Ttl : 0,
                                    BufferLength = p.Buffer.Length,
                                    Status = p.Status.ToString(),
                                };
                            }
                    ),
                });
            }
        }
    }
}
