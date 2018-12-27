using EventsDomain;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace RandomDataGenerator
{
    class Generator
    {
        private readonly Random _randomizer = new Random();
        private readonly string _filename;
        private readonly TimeSpan _interval;
        private readonly TicksGetter _ticker = new TicksGetter();
        private readonly ConcurrentQueue<RealEvent> _queue = new ConcurrentQueue<RealEvent>();
        private readonly AutoResetEvent _gcEventsCatched = new AutoResetEvent(false);
        private readonly List<RealEvent> _allEvents = new List<RealEvent>();
        private volatile bool _extrapolationCompleted = false;

        public Generator(string filename, TimeSpan interval)
        {
            _filename = filename;
            _interval = interval;
        }

        public void GenerateFile(int desiredEventsCount)
        {
            using (var watcher = new GCNotificationWatcher())
            {
                watcher.OnGarbageCollectionStarted += AddStartEvent;
                watcher.OnGarbageCollectionEnded += AddStopEvent;
                watcher.Start();

                var heavyMetal = new ObjectAllocator();
                heavyMetal.Start();

                var timer = new Timer(state =>
                {
                    _gcEventsCatched.Set();
                    heavyMetal.Stop();
                }, null, (long)_interval.TotalMilliseconds, Timeout.Infinite);

                WriteFile(() => heavyMetal.IsRunning, false);

                timer.Dispose();
            }

            Console.WriteLine("\nExtrapolation started...");

            StartDataExtrapolation(desiredEventsCount);
            
            WriteFile(() => !_extrapolationCompleted, true);

        }
        
        private void StartDataExtrapolation(int desiredEventsCount)
        {
            var thread = new Thread(() =>
            {
                var random = new Random();
                var spinWait = new SpinWait();
                int totalEventsGenerated = 0;
                long lastEventTime = _allEvents[_allEvents.Count - 1].Ticks;
                desiredEventsCount -= _allEvents.Count;
            

                while (totalEventsGenerated < desiredEventsCount)
                {
                    var baseIndex = random.Next(0, _allEvents.Count);
                    if (_allEvents[baseIndex].EventType == EventType.Stop)
                        baseIndex++;

                    if (baseIndex >= _allEvents.Count)
                        continue;

                    long diff = Math.Abs(_allEvents[baseIndex].Ticks - lastEventTime) + 100;

                    for (var i = baseIndex; i < _allEvents.Count; i++)
                    {
                        lastEventTime = diff + _allEvents[i].Ticks;
                        _queue.Enqueue(new RealEvent(_allEvents[i].EventType, lastEventTime));
                        totalEventsGenerated++;
                    }

                    _gcEventsCatched.Set();
                    
                    while (_queue.Count > 1000)
                        spinWait.SpinOnce();
                    
                    spinWait.Reset();
                }

                _extrapolationCompleted = true;
            });
            thread.Start();
        }

        private void AddStartEvent()
        {
            var ticks = _ticker.Peek();
            var realEvent = new RealEvent(EventType.Start, ticks);
            _queue.Enqueue(realEvent);
            _allEvents.Add(realEvent);
        }

        private void AddStopEvent()
        {
            var ticks = _ticker.Peek();
            var realEvent = new RealEvent(EventType.Stop, ticks);
            _queue.Enqueue(realEvent);
            _allEvents.Add(realEvent);
            if (_queue.Count >= 10)
                _gcEventsCatched.Set();
        }

        private void WriteFile(Func<bool> continueWriting, bool append)
        {
            var thread = new Thread(() =>
            {
                using (var fileWriter = new FileWriter(_filename, append))
                {
                    int writtenEvents = 0;
                    while (continueWriting())
                    {
                        _gcEventsCatched.WaitOne();
                        while (_queue.TryDequeue(out RealEvent ev))
                        {
                            fileWriter.WriteEvent(
                                ev,
                                new Payload(GetRandomNumber(), GetRandomNumber(), GetRandomNumber(), GetRandomNumber())
                            );
                            writtenEvents++;
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
