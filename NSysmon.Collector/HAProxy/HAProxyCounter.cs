using System.Threading;

namespace NSysmon.Collector.HAProxy
{
    public class HAProxyCounter
    {
        public HAProxyCounter(string key)
        {
            this.key = key;
        }

        private readonly string key;
        public string Key { get { return key; } }

        private long countHTTPRequestTotal;
        private long countHTTPRequest1xx;
        private long countHTTPRequest2xx;
        private long countHTTPRequest3xx;
        private long countHTTPRequest4xx;
        private long countHTTPRequest5xx;
        private long countHTTPRequestOther;
        private long sumHTTPContentLength;
        private long sumHTTPBytesRead;
        private long sumTq;
        private long sumTw;
        private long sumTc;
        private long sumTr;
        private long sumTt;

        public long CountHTTPRequestTotal { get { return this.countHTTPRequestTotal; } }
        public long CountHTTPRequest1xx { get { return this.countHTTPRequest1xx; } }
        public long CountHTTPRequest2xx { get { return this.countHTTPRequest2xx; } }
        public long CountHTTPRequest3xx { get { return this.countHTTPRequest3xx; } }
        public long CountHTTPRequest4xx { get { return this.countHTTPRequest4xx; } }
        public long CountHTTPRequest5xx { get { return this.countHTTPRequest5xx; } }
        public long CountHTTPRequestOther { get { return this.countHTTPRequestOther; } }
        public long SumHTTPContentLength { get { return this.sumHTTPContentLength; } }
        public long SumHTTPBytesRead { get { return this.sumHTTPBytesRead; } }
        public long SumTq { get { return this.sumTq; } }
        public long SumTw { get { return this.sumTw; } }
        public long SumTc { get { return this.sumTc; } }
        public long SumTr { get { return this.sumTr; } }
        public long SumTt { get { return this.sumTt; } }

        public void Increment(Syslog.HAProxySyslogDatagram datagram)
        {
            Interlocked.Increment(ref countHTTPRequestTotal);
            if (datagram.Status_Code >= 100 && datagram.Status_Code < 200)
            {
                Interlocked.Increment(ref countHTTPRequest1xx);
            }
            else if (datagram.Status_Code >= 200 && datagram.Status_Code < 300)
            {
                Interlocked.Increment(ref countHTTPRequest2xx);
            }
            else if (datagram.Status_Code >= 300 && datagram.Status_Code < 400)
            {
                Interlocked.Increment(ref countHTTPRequest3xx);
            }
            else if (datagram.Status_Code >= 400 && datagram.Status_Code < 500)
            {
                Interlocked.Increment(ref countHTTPRequest4xx);
            }
            else if (datagram.Status_Code >= 500 && datagram.Status_Code < 600)
            {
                Interlocked.Increment(ref countHTTPRequest5xx);
            }
            else
            {
                Interlocked.Increment(ref countHTTPRequestOther);
            }
            Interlocked.Add(ref sumHTTPContentLength, datagram.Content_Length);
            Interlocked.Add(ref sumHTTPBytesRead, datagram.Bytes_Read);
            Interlocked.Add(ref sumTq, datagram.Tq);
            Interlocked.Add(ref sumTw, datagram.Tw);
            Interlocked.Add(ref sumTc, datagram.Tc);
            Interlocked.Add(ref sumTr, datagram.Tr);
            Interlocked.Add(ref sumTt, datagram.Tt);
        }
    }
}
