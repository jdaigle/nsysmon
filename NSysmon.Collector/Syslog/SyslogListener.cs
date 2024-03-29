﻿using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace NSysmon.Collector.Syslog
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
                        string syslogDatagram = "";
                        try
                        {
                            syslogDatagram = Encoding.ASCII.GetString(receiveBuffer);
                            Log.DebugFormat("Received Datagram From IP [{0}] : {1}", sourceIPAddress.ToString(), syslogDatagram);
                            var datagram = SyslogDatagramParser.Parse(syslogDatagram, sourceIPAddress.ToString());
                            if (datagram != null)
                            {
                                datagram.Handle();
                            }
                        }
                        catch (Exception e)
                        {
                            Log.Error("Error Handling Syslog Message: [" + syslogDatagram + "]", e);
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
