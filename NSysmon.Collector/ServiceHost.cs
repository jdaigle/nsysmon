using System;
using System.IO;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Web.Http.SelfHost;
using NSysmon.Collector.Api;
using NSysmon.Collector.Syslog;

namespace NSysmon.Collector
{
    public class ServiceHost
    {
        public class JSONConfig
        {
            public JSONConfig()
            {
                nodes = new JSONConfigNode[0];
                syslog = new JSONConfigSyslog();
                http = new JSONConfigHttp();
            }

            public JSONConfigNode[] nodes { get; set; }
            public JSONConfigSyslog syslog { get; set; }
            public JSONConfigHttp http { get; set; }
        }

        public class JSONConfigNode
        {
            public string type { get; set; }
            public string name { get; set; }
            public object settings { get; set; }
        }

        public class JSONConfigSyslog
        {
            public JSONConfigSyslog()
            {
                listenerEnabled = true;
            }

            public bool listenerEnabled { get; set; }
            public int listenerPort { get; set; }
        }

        public class JSONConfigHttp
        {
            public JSONConfigHttp()
            {
                listenerPort = 8080;
            }

            public int listenerPort { get; set; }
        }

        private HttpSelfHostServer httpServer;

        public void Stop()
        {
            // stop server
            if (httpServer != null)
            {
                httpServer.CloseAsync().Wait();
            }
            // stop polling
            PollingEngine.StopPolling();
        }

        public void Start()
        {
            // start the perfmon ring buffer
            PerfMon.Instance.Init();

            var nodeConfigString = File.ReadAllText("config.json");
            var settings = new Newtonsoft.Json.JsonSerializerSettings
            {
                TypeNameHandling = Newtonsoft.Json.TypeNameHandling.Auto,
            };
            var config = Newtonsoft.Json.JsonConvert.DeserializeObject<JSONConfig>(nodeConfigString, settings);
            foreach (var node in (config.nodes ?? new JSONConfigNode[0]))
            {
                var pollNode = Activator.CreateInstance(typeof(PollingEngine).Assembly.GetType(node.type), new object[] { node.name, node.settings }) as PollNode;
                PollingEngine.TryAdd(pollNode);
            }
            PollingEngine.StartPolling();

            if (config.syslog.listenerEnabled)
            {
                var listener = new SyslogListener();
                Action<int> startListener = listener.Start;
                startListener.BeginInvoke(514, null, null);
            }

            var hostConfig = new HttpSelfHostConfiguration("http://127.0.0.1:" + config.http.listenerPort);
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
            hostConfig.Filters.Add(new ApiExceptionFilterAttribute());

            httpServer = new HttpSelfHostServer(hostConfig);
            httpServer.OpenAsync().Wait();
        }
    }
}
