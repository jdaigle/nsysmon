using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Disruptor;
using Disruptor.Dsl;

namespace NSysmon.Core.PerfMon
{
    public static class Instance
    {
        private static readonly int ringSize = 1024;  // Must be multiple of 2

        public static void Init()
        {
            var disruptor = new Disruptor<CounterValue>(() => new CounterValue(), ringSize, TaskScheduler.Default);
            disruptor.HandleEventsWith(new CounterValueHandler());
            ringBuffer = disruptor.Start();
        }

        private static RingBuffer<CounterValue> ringBuffer;

        public static void Publish(string host, DateTime timestamp, string category, string counter, string instance, float value)
        {
            var seq = ringBuffer.Next();
            var entry = ringBuffer[seq];
            entry.Host = host;
            entry.Timestamp = timestamp;
            entry.Category = category;
            entry.Counter = counter;
            entry.Instance = instance;
            entry.Value = value;
            var d = Convert.ToDecimal(value);
            ringBuffer.Publish(seq);
        }
    }
}
