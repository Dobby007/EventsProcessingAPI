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
        protected readonly Random _randomizer = new Random();
        protected readonly TicksGetter _ticker = new TicksGetter();
        protected readonly ConcurrentQueue<RealEvent> _queue = new ConcurrentQueue<RealEvent>();
        protected readonly AutoResetEvent _eventsBatchGenerated = new AutoResetEvent(false);

        protected DataGenerator(string filename)
        {
            _filename = filename;
        }

        protected void WriteFile(Func<bool> continueWriting, bool append)
        {
            var thread = new Thread(() =>
            {
                using (var fileWriter = new FileWriter(_filename, append))
                {
                    int writtenEvents = 0;
                    RealEvent lastEventWritten = default;
                    while (continueWriting() || !_queue.IsEmpty)
                    {
                        _eventsBatchGenerated.WaitOne();
                        while (_queue.TryDequeue(out RealEvent ev))
                        {
                            if (writtenEvents > 0 && ev.EventType == lastEventWritten.EventType)
                                throw new InvalidOperationException();

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

        private long GetRandomNumber()
        {
            return (long)(_randomizer.NextDouble() * long.MaxValue);
        }
    }
}
