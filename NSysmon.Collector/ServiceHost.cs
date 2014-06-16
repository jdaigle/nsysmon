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
            var nodeConfigString = File.ReadAllText("config.json");
            var settings = new Newtonsoft.Json.JsonSerializerSettings
            {
                TypeNameHandling = Newtonsoft.Json.TypeNameHandling.Auto,
            };
            var config = Newtonsoft.Json.JsonConvert.DeserializeObject<JSONConfig>(nodeConfigString, settings);

            // config Syslog listener
            if (config.syslog.listenerEnabled)
            {
                var listener = new SyslogListener();
                Action<int> startListener = listener.Start;
                startListener.BeginInvoke(config.syslog.listenerPort, null, null);
            }

            // config PollEngine
            foreach (var node in (config.nodes ?? new JSONConfigNode[0]))
            {
                var pollNode = Activator.CreateInstance(typeof(PollingEngine).Assembly.GetType(node.type), new object[] { node.name, node.settings }) as PollNode;
                PollingEngine.TryAdd(pollNode);
            }
            PollingEngine.StartPolling();

            // config HTTP API
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
