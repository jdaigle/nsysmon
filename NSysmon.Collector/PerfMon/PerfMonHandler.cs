using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Disruptor;
using NWhisper;

namespace NSysmon.Collector.PerfMon
{
    public class PerfMonHandler : IEventHandler<CounterValue>
    {
        private CounterDictionary counterDictionary;
            
        public PerfMonHandler()
        {
            this.counterDictionary = CounterDictionary.ReadFromFile(@"c:\temp\counters\index.dat");
            NWhisper.Whipser.CacheHeaders = true;
        }

        public void OnNext(CounterValue data, long sequence, bool endOfBatch)
        {
            var counterId = this.counterDictionary.GetCounterIdOrNew(data.Host, data.Category, data.Counter, data.Instance);
            var path = string.Format(@"c:\temp\counters\{0}.dat", counterId);
            ValidateCounterExists(path);
            try
            {
                Whipser.Update(path, Convert.ToDouble(data.Value), timestamp: data.Timestamp.ToUniversalTime().ToUnixTime());
            }
            catch (Exception e)
            {
                e.ToString();
                throw;
            }
        }

        private static readonly Dictionary<string, Header> knownCounterPaths = new Dictionary<string, Header>();

        private static void ValidateCounterExists(string path)
        {
            if (knownCounterPaths.ContainsKey(path))
            {
                return;
            }
            if (File.Exists(path))
            {
                try
                {
                    knownCounterPaths.Add(path, Whipser.Info(path));
                    return;
                }
                catch (CorruptWhisperFileException)
                {
                    File.Delete(path);
                    // corrupt?
                }
            }
            var retention = new List<ArchiveInfo>() { 
                new ArchiveInfo(10, 180), // 10 seconds * 180 points = 30 min = 2.9 kb
                new ArchiveInfo(60, 60), // 60 seconds (1min) * 60 points = 1 hr = 3.9 kb
            };
            Whipser.Create(path, retention);
            knownCounterPaths.Add(path, Whipser.Info(path));
        }
    }
}
