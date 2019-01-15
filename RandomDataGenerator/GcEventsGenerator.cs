using EventsDomain;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace RandomDataGenerator
{
    class GcEventsGenerator : DataGenerator
    {
        private readonly AutoResetEvent _gcEventsCatched = new AutoResetEvent(false);
        private readonly List<RealEvent> _allEvents = new List<RealEvent>();
        private volatile bool _extrapolationCompleted = false;
        
        private readonly TimeSpan _interval;
        private readonly AllocationMode _allocationMode;

        public GcEventsGenerator(string filename, TimeSpan interval, AllocationMode allocationMode)
            : base(filename)
        {
            _interval = interval;
            _allocationMode = allocationMode;
        }

        public void GenerateFile(int desiredEventsCount)
        {
            using (var watcher = new GCNotificationWatcher())
            {
                watcher.OnGarbageCollectionStarted += AddStartEvent;
                watcher.OnGarbageCollectionEnded += AddStopEvent;
                watcher.Start();

                

                var timer = new Timer(state =>
                {
                    heavyMetal.Stop();
                    _gcEventsCatched.Set();
                }, null, (long)_interval.TotalMilliseconds, Timeout.Infinite);

                WriteFile(() => heavyMetal.IsRunning && !heavyMetal.AllocationCompleted, false);

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

        
        
    }
}
