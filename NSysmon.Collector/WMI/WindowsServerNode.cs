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

        public WindowsServerNode(string nodeName, IEnumerable<string> groups, WindowsServerNodeSettings settings)
            : base(nodeName, groups)
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
                yield return TcpipNetworkInterfaces;
            }
        }

        public bool IsWindowsServer2008CompatOrBetter()
        {
            return char.IsNumber(TryGetOSVersion()[0]) && int.Parse(TryGetOSVersion()[0].ToString()) >= 6;
        }

        public string TryGetOSVersion()
        {
            if (_win32ComputerSystem != null
                && _win32ComputerSystem.ContainsCachedData()
                && _win32ComputerSystem.Data != null
                && _win32ComputerSystem.Data.OSVersion != "")
            {
                return _win32ComputerSystem.Data.OSVersion;
            }
            return "Unknown";
        }

        private PollNodeDataCache<List<TcpipNetworkInterface>> _tcpipNetworkInterface;
        public PollNodeDataCache<List<TcpipNetworkInterface>> TcpipNetworkInterfaces
        {
            get
            {
                return _tcpipNetworkInterface ?? (_tcpipNetworkInterface = new PollNodeDataCache<List<TcpipNetworkInterface>>(
                    this
                    , () =>
                    {
                        if (!IsWindowsServer2008CompatOrBetter())
                        {
                            return new List<TcpipNetworkInterface>();
                        }
                        return
                            Instrumentation.Query(settings.Host, settings.WMIPollingSettings,
                            "select Name, Caption, Description, CurrentBandwidth from Win32_PerfFormattedData_Tcpip_NetworkInterface",
                            results => results.Select(mo => new TcpipNetworkInterface()
                            {
                                Name = (mo["name"] ?? string.Empty).ToString(),
                                Caption = (mo["Caption"] ?? string.Empty).ToString(),
                                Description = (mo["Description"] ?? string.Empty).ToString(),
                                CurrentBandwidth = (UInt64)mo["CurrentBandwidth"],
                            })).Data.ToList();
                    }
                    , 60 * 5 // cache for 5 minutes
                    , description: string.Format("WMI Query Win32_PerfFormattedData_Tcpip_NetworkInterface On Computer {0} ", settings.Host)
                    ));
            }
        }

        private PollNodeDataCache<List<Win32Volume>> _win32Volumes;
        public PollNodeDataCache<List<Win32Volume>> Win32Volumes
        {
            get
            {
                return _win32Volumes ?? (_win32Volumes = new PollNodeDataCache<List<Win32Volume>>(
                    this
                    , () => Instrumentation.Query(settings.Host, settings.WMIPollingSettings,
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
                    , 60 * 5 // cache for 5 minutes
                    , description: string.Format("WMI Query Win32_Volume On Computer {0} ", settings.Host)
                    ));
            }
        }

        private PollNodeDataCache<Win32ComputerSystem> _win32ComputerSystem;
        public PollNodeDataCache<Win32ComputerSystem> Win32ComputerSystem
        {
            get
            {
                return _win32ComputerSystem ?? (_win32ComputerSystem = new PollNodeDataCache<Win32ComputerSystem>(
                    this
                    , () =>
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
                    , 60 * 60 // cache for 60 minutes
                    , description: string.Format("WMI Query Win32_ComputerSystem/Win32_OperatingSystem On Computer {0} ", settings.Host)
                    ));
            }
        }

        private PollNodeDataCache<List<Win32NetworkAdapter>> _win32NetworkAdapters;
        public PollNodeDataCache<List<Win32NetworkAdapter>> Win32NetworkAdapters
        {
            get
            {
                return _win32NetworkAdapters ?? (_win32NetworkAdapters = new PollNodeDataCache<List<Win32NetworkAdapter>>(
                    this
                    , () =>
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
                    , 60 * 5 // cache for 5 minutes
                    , description: string.Format("WMI Query Win32_NetworkAdapter On Computer {0} ", settings.Host)
                    ));
            }
        }

        private PollNodeDataCache<PingResult> _pingPoller;
        private System.Net.NetworkInformation.Ping pinger = new System.Net.NetworkInformation.Ping();
        public PollNodeDataCache<PingResult> PingPoller
        {
            get
            {
                return _pingPoller ?? (_pingPoller = new PollNodeDataCache<PingResult>(
                    this
                    , () =>
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
                    , 0 // no cache
                    , description: string.Format("Ping host {0} ", settings.Host)
                    ));
            }
        }
    }
}
