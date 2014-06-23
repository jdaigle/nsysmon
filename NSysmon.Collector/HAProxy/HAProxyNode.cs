using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace NSysmon.Collector.HAProxy
{
    public class HAProxyNode : PollNode
    {
        public override string NodeType { get { return "HAProxy"; } }
        public override int MinSecondsBetweenPolls { get { return 5; } }

        private readonly HAProxyNodeSettings settings;
        public string NodeName { get; private set; }

        public HAProxyNode(string nodeName, IEnumerable<string> groups, HAProxyNodeSettings settings)
            : base(nodeName, groups)
        {
            this.NodeName = nodeName;
            this.settings = settings;
            this.counters = new ConcurrentDictionary<string, HAProxyCounter>(StringComparer.InvariantCultureIgnoreCase);
        }

        public override IEnumerable<PollNodeDataCache> DataCaches
        {
            get
            {
                yield return Proxies;
            }
        }

        private PollNodeDataCache<List<Proxy>> _proxies;
        public PollNodeDataCache<List<Proxy>> Proxies
        {
            get
            {
                return _proxies ?? (_proxies = new PollNodeDataCache<List<Proxy>>(
                    this
                    , FetchHAProxyStats
                    , 10 // cache for 10 seconds
                    , description: string.Format("Query HAProxy Stats {0} ", settings.Url)
                    ));
            }
        }

        private List<Proxy> FetchHAProxyStats()
        {
            string csv;
            var req = (HttpWebRequest)WebRequest.Create(this.settings.Url + ";csv");
            //req.Credentials = new NetworkCredential(User, Password);
            req.Timeout = this.settings.QueryTimeout;
            using (var resp = req.GetResponse())
            using (var rs = resp.GetResponseStream())
            {
                if (rs == null)
                {
                    return null;
                }
                using (var sr = new StreamReader(rs))
                {
                    csv = sr.ReadToEnd();
                }
            }
            return ParseHAProxyStats(csv);
        }

        private List<Proxy> ParseHAProxyStats(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return new List<Proxy>();
            }
            var lines = input.Split(new[] { '\n' });

            var stats = new List<Item>();
            foreach (var l in lines)
            {
                //Skip the header
                if (string.IsNullOrEmpty(l) || l.StartsWith("#")) continue;
                //Collect each stat line as we go, group later
                stats.Add(Item.FromLine(l));
            }
            var result = stats.GroupBy(s => s.UniqueProxyId).Select(g => new Proxy
            {
                Instance = this,
                Name = g.First().ProxyName,
                Frontend = g.FirstOrDefault(s => s.Type == StatusType.Frontend) as Frontend,
                Servers = g.OfType<Server>().ToList(),
                Backend = g.FirstOrDefault(s => s.Type == StatusType.Backend) as Backend,
                PollDate = DateTime.UtcNow
            }).ToList();

            return result;
        }

        private ConcurrentDictionary<string, HAProxyCounter> counters;

        public IReadOnlyList<HAProxyCounter> Counters { get { return this.counters.Values.ToList().AsReadOnly(); } }

        public void Count(string key, Syslog.HAProxySyslogDatagram datagram)
        {
            HAProxyCounter counter;
            if (!counters.TryGetValue(key, out counter))
            {
                counters.TryAdd(key, new HAProxyCounter(key));
                counters.TryGetValue(key, out counter);
            }
            counter.Increment(datagram);
        }
    }
}
