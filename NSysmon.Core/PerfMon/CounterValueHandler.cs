using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Disruptor;

namespace NSysmon.Core.PerfMon
{
    public class CounterValueHandler: IEventHandler<CounterValue>
    {
        public void OnNext(CounterValue data, long sequence, bool endOfBatch)
        {
            Console.WriteLine(data.ToString());
        }
    }
}
