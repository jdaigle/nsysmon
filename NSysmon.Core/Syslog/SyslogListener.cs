using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace NSysmon.Core.Syslog
{
    public class SyslogListener
    {
        public static log4net.ILog Log = log4net.LogManager.GetLogger(typeof(ServiceHost));

        public void Start(int port)
        {
            var udpListener = new UdpClient(port);
            Log.InfoFormat("Starting Syslog Lisener on Port {0}", port);
            while (true)
            {
                try
                {
                    var sourceIPAddress = new IPEndPoint(IPAddress.Any, 0);
                    var receiveBuffer = udpListener.Receive(ref sourceIPAddress);
                    Task.Factory.StartNew(() =>
                    {
                        try
                        {
                            var syslogDatagram = Encoding.ASCII.GetString(receiveBuffer);
                            Log.DebugFormat("Received Datagram From IP [{0}] : {1}", sourceIPAddress.ToString(), syslogDatagram);
                            var datagram = SyslogDatagramParser.Parse(syslogDatagram, sourceIPAddress.ToString());
                            //var haproxySyslogMessage = HAProxySyslogMessage.Parse(syslogMessage, sourceIPAddress.ToString());
                            //if (haproxySyslogMessage != null)
                            //{
                            //    Log.Debug("Message is HAProxy: " + syslogMessage);
                            //    using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["HAProxyLogs"].ConnectionString))
                            //    {
                            //        connection.Open();
                            //        haproxySyslogMessage.WriteToDatabase(connection);
                            //    }
                            //}
                            //else
                            //{
                            //    Log.Debug("Message is not HAProxy: " + syslogMessage);
                            //}
                        }
                        catch (Exception e)
                        {
                            Log.Error(e);
                            throw;
                        }
                    });
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }
    }
}
