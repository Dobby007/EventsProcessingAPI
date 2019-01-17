using EventsDomain;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace RandomDataGenerator
{
    abstract class DataGenerator
    {
        protected readonly string _filename;
        private readonly Random _randomizer = new Random();
        protected readonly TicksGetter _ticker = new TicksGetter();
        protected ConcurrentQueue<RealEvent> _queue;
        protected readonly AutoResetEvent _eventsBatchGenerated = new AutoResetEvent(false);

        protected DataGenerator(string filename)
        {
            _filename = filename;
            InitQueue();
        }

        protected void WriteFile(Func<bool> continueWriting, bool append)
        {
            var thread = new Thread(() =>
            {
                using (var fileWriter = new FileWriter(_filename, append))
                {
                    int writtenEvents = 0;
                    RealEvent lastEventWritten = default;
                    bool continueFlag = true;
                    while ((continueFlag = continueWriting()) || !_queue.IsEmpty)
                    {
                        // we don't need to wait if there will be no more events
                        if (continueFlag && !_queue.IsEmpty)
                            _eventsBatchGenerated.WaitOne(3000); 

                        while (_queue.TryDequeue(out RealEvent ev))
                        {
                            // start event should be first event ever
                            if (writtenEvents == 0 && ev.EventType == EventType.Stop)
                                continue;

                            if (writtenEvents > 0 && ev.Ticks <= lastEventWritten.Ticks)
                                throw new InvalidOperationException($"Events are not sorted. Previous event: {lastEventWritten}, current event: {ev}");

                            if (writtenEvents > 0 && ev.EventType == lastEventWritten.EventType)
                                throw new InvalidOperationException($"Event types are the same. Previous event: {lastEventWritten}, current event: {ev}");

                            fileWriter.WriteEvent(
                                ev,
                                new Payload(GetRandomNumber(), GetRandomNumber(), GetRandomNumber(), GetRandomNumber())
                            );
                            writtenEvents++;
                            lastEventWritten = ev;
                        }
                        Console.Write("\rNumber of events written: {0}", writtenEvents);
                    }
                }
            });
            thread.Start();
            thread.Join();
        }

        protected void InitQueue()
        {
            _queue = new ConcurrentQueue<RealEvent>();
        }

        private long GetRandomNumber()
        {
            return (long)(_randomizer.NextDouble() * long.MaxValue);
        }
    }
}
