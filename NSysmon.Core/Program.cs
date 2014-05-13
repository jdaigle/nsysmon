using System;
using System.IO;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Web.Http.SelfHost;
using NSysmon.Core.Api;

namespace NSysmon.Core
{
    class Program
    {
        public class JSONConfig
        {
            public JSONConfigNode[] nodes { get; set; }
        }

        public class JSONConfigNode
        {
            public string type { get; set; }
            public string name { get; set; }
            public object settings { get; set; }
        }

        static void Main(string[] args)
        {
            log4net.Config.XmlConfigurator.Configure();

            var nodeConfigString = File.ReadAllText("config.json");
            var settings = new Newtonsoft.Json.JsonSerializerSettings
            {
                TypeNameHandling = Newtonsoft.Json.TypeNameHandling.Auto,
            };
            var nodeConfig = Newtonsoft.Json.JsonConvert.DeserializeObject<JSONConfig>(nodeConfigString, settings);
            foreach (var node in (nodeConfig.nodes ?? new JSONConfigNode[0]))
            {
                var pollNode = Activator.CreateInstance(typeof(PollingEngine).Assembly.GetType(node.type), new object[] { node.name, node.settings }) as PollNode;
                PollingEngine.TryAdd(pollNode);
            }
            PollingEngine.StartPolling();

            var hostConfig = new HttpSelfHostConfiguration("http://localhost:8080");
            // Remove the XML formatter
            hostConfig.Formatters.Remove(hostConfig.Formatters.XmlFormatter);
            hostConfig.Formatters.Add(new RazorFormatter());
            hostConfig.Formatters.JsonFormatter.SerializerSettings.TypeNameHandling = Newtonsoft.Json.TypeNameHandling.Auto;
            hostConfig.Formatters.JsonFormatter.SerializerSettings.Formatting = Newtonsoft.Json.Formatting.Indented;
            hostConfig.Formatters.JsonFormatter.SerializerSettings.Error += (x, y) =>
            {
                return;
            };
            hostConfig.Formatters.JsonFormatter.MediaTypeMappings.Add(new QueryStringMapping("type", "json", new MediaTypeHeaderValue("application/json")));
            // Attribute routing.
            hostConfig.MapHttpAttributeRoutes();
            hostConfig.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;

            using (var server = new HttpSelfHostServer(hostConfig))
            {
                server.OpenAsync().Wait();
                Console.WriteLine("Press Enter to quit.");
                Console.ReadLine();
            }
        }
    }
}
