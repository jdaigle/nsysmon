using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web.Http;
using System.Web.Http.SelfHost;
using System.Threading.Tasks;
using NSysmon.Core.WMI;
using NSysmon.Core.SqlServer;

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
            var config = new HttpSelfHostConfiguration("http://localhost:8080");
            // Remove the XML formatter
            config.Formatters.Remove(config.Formatters.XmlFormatter);
            config.Formatters.JsonFormatter.SerializerSettings.Formatting = Newtonsoft.Json.Formatting.Indented;
            config.Formatters.JsonFormatter.SerializerSettings.Error += (x, y) =>
            {
                return;
            };
            // Attribute routing.
            config.MapHttpAttributeRoutes();
            config.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;

            using (var server = new HttpSelfHostServer(config))
            {
                server.OpenAsync().Wait();
                Console.WriteLine("Press Enter to quit.");
                Console.ReadLine();
            }

            while (true)
            {
                Thread.Sleep(2000);
            }
        }
    }
}
