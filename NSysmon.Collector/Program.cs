﻿using System;
using System.IO;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Web.Http.SelfHost;
using NSysmon.Collector.Api;
using Topshelf;

namespace NSysmon.Collector
{
    public class Program
    {
        public static void Main(string[] args)
        {
            log4net.Config.XmlConfigurator.Configure();
            var returnCode = HostFactory.Run(x =>
            {
                x.Service<ServiceHost>(s =>
                {
                    s.ConstructUsing(name => new ServiceHost());
                    s.WhenStarted(tc => tc.Start());
                    s.WhenStopped(tc => tc.Stop());
                });
                x.RunAsPrompt();
                x.StartAutomaticallyDelayed();
                x.SetDescription("NSysmon Collector");
                x.SetDisplayName("NSysmon Collector");
                x.SetServiceName("NSysmonCollector");
            });
        }
    }
}
