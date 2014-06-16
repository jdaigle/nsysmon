using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWhisper;

namespace NSysmon.Collector.PerfMon
{
    public class PerfMonFile
    {
        public static readonly PerfMonFile Instance = new PerfMonFile();

        private CounterDictionary counterDictionary;
            
        private PerfMonFile()
        {
            this.counterDictionary = CounterDictionary.ReadFromFile(@"c:\temp\counters\index.dat");
            NWhisper.Whipser.CacheHeaders = true;
        }

        public void Publish(string host, DateTime timestamp, string category, string counter, string instance, float value)
        {
            var counterId = this.counterDictionary.GetCounterIdOrNew(host, category, counter, instance);
            var path = string.Format(@"c:\temp\counters\{0}.dat", counterId);
            ValidateCounterExists(path);
            try
            {
                Whipser.Update(path, Convert.ToDouble(value), timestamp: timestamp.ToUniversalTime().ToUnixTime());
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
