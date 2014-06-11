using System;
using System.IO;
using Topshelf;

namespace NSysmon.Forwarder
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
                x.SetDescription("NSysmon Forwarder");
                x.SetDisplayName("NSysmon Forwarder");
                x.SetServiceName("NSysmonForwarder");
            });
        }
    }
}
