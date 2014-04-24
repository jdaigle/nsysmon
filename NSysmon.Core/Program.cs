using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NSysmon.Core
{
    class Program
    {
        static void Main(string[] args)
        {
            PollingEngine.StartPolling();
            PollingEngine.TryAdd(new ServerNode("192.168.10.10"));
            while (true)
            {
                Thread.Sleep(500);
                Console.WriteLine(PollingEngine.AllPollNodes.First().MonitorStatus + " | " + ((dynamic)PollingEngine.AllPollNodes.First().DataPollers.First().CachedData).Status + " | " + PollingEngine.AllPollNodes.First().LastPoll + " | " + PollingEngine.AllPollNodes.First().DataPollers.First().NextPoll);
            }
        }

        public class ServerNode : PollNode
        {
            private string ipaddress;

            public ServerNode(string ipaddress)
                : base(ipaddress)
            {
                this.ipaddress = ipaddress;
            }

            public override string NodeType { get { return "Server"; } }
            public override int MinSecondsBetweenPolls { get { return 5; } }

            public override IEnumerable<DataPoller> DataPollers
            {
                get
                {
                    yield return PingPoller;
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


            private DataPoller<System.Net.NetworkInformation.PingReply> _pingPoller;
            private static System.Net.NetworkInformation.Ping pinger = new System.Net.NetworkInformation.Ping();
            public DataPoller<System.Net.NetworkInformation.PingReply> PingPoller
            {
                get
                {
                    return _pingPoller ?? (_pingPoller = new DataPoller<System.Net.NetworkInformation.PingReply>()
                    {
                        //CacheForSeconds = 10,
                        UpdateCachedData = UpdateDataPollerCachedData(
                            description: "Ping: " + ipaddress,
                            getData: () => pinger.Send(ipaddress)
                        ),
                    });
                }
            }

        }
    }
}
