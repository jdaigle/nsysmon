using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Disruptor;

namespace NSysmon.Core.PerfMon
{
    public class PerfMonHandler : IEventHandler<CounterValue>
    {
        private CounterDictionary counterDictionary;

        public PerfMonHandler()
        {
            this.counterDictionary = CounterDictionary.ReadFromFile(@"c:\temp\counters\index.dat");
        }

        public void OnNext(CounterValue data, long sequence, bool endOfBatch)
        {
            var counterId = this.counterDictionary.GetCounterIdOrNew(data.Host, data.Category, data.Counter, data.Instance);
        }
    }
}
