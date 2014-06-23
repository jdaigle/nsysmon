using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSysmon.Collector.Syslog
{
    public class HAProxySyslogDatagram : SyslogDatagram
    {
        public HAProxySyslogDatagram(string datagram, int severity, int facility, DateTime sentDateTime, string sourceIPAddress)
            : base(datagram, severity, facility, sentDateTime, sourceIPAddress) { }

        public string Node_Name;

        public string Pid;

        public string Client_IP;
        public int Client_Port;

        public DateTime Accept_Date;

        public string Frontend_Name;
        public string Backend_Name;
        public string Server_Name;

        /// <summary>
        /// total time in milliseconds spent waiting for the client to send a full HTTP request, not counting data.
        /// It can be "-1" if the connection
        /// was aborted before a complete request could be received. It should always
        /// be very small because a request generally fits in one single packet. Large
        /// times here generally indicate network trouble between the client and
        /// haproxy.
        /// </summary>
        public int Tq;
        /// <summary>
        /// total time in milliseconds spent waiting in the various queues.
        /// It can be "-1" if the connection was aborted before reaching the queue.
        /// </summary>
        public int Tw;
        /// <summary>
        /// total time in milliseconds spent waiting for the connection to
        /// establish to the final server, including retries. It can be "-1" if the
        //// request was aborted before a connection could be established.
        /// </summary>
        public int Tc;
        /// <summary>
        /// total time in milliseconds spent waiting for the server to send a full HTTP response, not counting data.
        /// It can be "-1" if the request was aborted before a complete response could be received.
        /// </summary>
        public int Tr;
        /// <summary>
        /// total time in milliseconds elapsed between the accept and the last close.
        /// </summary>
        public int Tt;

        public int Status_Code;

        /// <summary>
        /// total number of bytes transmitted to the client when the log is emitted. This does include HTTP headers.
        /// </summary>
        public long Bytes_Read;

        public string Captured_Request_Cookie;

        public string Captured_Response_Cookie;

        public string Termination_State;
        public char Termination_State_SessionErrorCode;
        public char Termination_State_SessionStateCode;
        public char Termination_State_PersistenceCookieCode;
        public char Termination_State_PersistenceCookieOpCode;

        /// <summary>
        /// total number of concurrent connections on the process when
        /// the session was logged. It is useful to detect when some per-process system
        /// limits have been reached. For instance, if actconn is close to 512 or 1024
        /// when multiple connection errors occur, chances are high that the system
        /// limits the process to use a maximum of 1024 file descriptors and that all
        /// of them are used. See section 3 "Global parameters" to find how to tune the
        /// system.
        /// </summary>
        public int Actconn;
        /// <summary>
        /// total number of concurrent connections on the frontend when
        /// the session was logged. It is useful to estimate the amount of resource
        /// required to sustain high loads, and to detect when the frontend's "maxconn"
        /// has been reached. Most often when this value increases by huge jumps, it is
        /// because there is congestion on the backend servers, but sometimes it can be
        /// caused by a denial of service attack.
        /// </summary>
        public int Feconn;
        /// <summary>
        /// total number of concurrent connections handled by the
        /// backend when the session was logged. It includes the total number of
        /// concurrent connections active on servers as well as the number of
        /// connections pending in queues. It is useful to estimate the amount of
        /// additional servers needed to support high loads for a given application.
        /// Most often when this value increases by huge jumps, it is because there is
        /// congestion on the backend servers, but sometimes it can be caused by a
        /// denial of service attack.
        /// </summary>
        public int Beconn;
        /// <summary>
        /// total number of concurrent connections still active on
        /// the server when the session was logged. It can never exceed the server's
        /// configured "maxconn" parameter. If this value is very often close or equal
        /// to the server's "maxconn", it means that traffic regulation is involved a
        /// lot, meaning that either the server's maxconn value is too low, or that
        /// there aren't enough servers to process the load with an optimal response
        /// time. When only one of the server's "srv_conn" is high, it usually means
        /// that this server has some trouble causing the requests to take longer to be
        /// processed than on other servers.
        /// </summary>
        public int Srv_conn;
        /// <summary>
        /// number of connection retries experienced by this session
        /// when trying to connect to the server. It must normally be zero, unless a
        /// server is being stopped at the same moment the connection was attempted.
        /// Frequent retries generally indicate either a network problem between
        /// haproxy and the server, or a misconfigured system backlog on the server
        /// preventing new connections from being queued.
        /// </summary>
        public int Retries;

        /// <summary>
        /// total number of requests which were processed before
        /// this one in the server queue. It is zero when the request has not gone
        /// through the server queue.
        /// </summary>
        public int Srv_Queue;
        /// <summary>
        /// total number of requests which were processed before
        /// this one in the backend's global queue. It is zero when the request has not
        /// gone through the global queue.
        /// </summary>
        public int Backend_Queue;

        public string Captured_Request_Headers;
        public string Captured_Response_Headers;
        
        // Request Headers
        public string User_Agent;
        public string Referer;
        public string Host;
        public string X_Forwarded_For;
        public string Accept_Encoding;

        // Response Headers
        public string Content_Encoding;

        public string HTTP_Request;
        public string HTTP_Request_Method;
        public string HTTP_Request_URL;
        public string HTTP_Request_Query;
        public string HTTP_Request_Version;
    }
}
