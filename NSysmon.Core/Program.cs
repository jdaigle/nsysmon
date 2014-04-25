using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NSysmon.Core.WMI;

namespace NSysmon.Core
{
    class Program
    {
        static void Main(string[] args)
        {
            log4net.Config.XmlConfigurator.Configure();
            PollingEngine.StartPolling();
            PollingEngine.TryAdd(new WindowsServerNode("127.0.0.1", new WindowsServerNodeSettings()
            {
                Host = "127.0.0.1"
            }));

            while (true)
            {
                Thread.Sleep(2000);
            }
        }
    }
}
