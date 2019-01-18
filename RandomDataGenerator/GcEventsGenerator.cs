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
        private readonly List<RealEvent> _allEvents = new List<RealEvent>();
        private volatile bool _extrapolationCompleted = false;
        
        private readonly GenerateOptions _options;

        public GcEventsGenerator(GenerateOptions options)
            : base(options.File)
        {
            _options = options;
        }

        public void GenerateFile()
        {
            using (var watcher = new GCNotificationWatcher())
            {
                watcher.OnGarbageCollectionStarted += AddStartEvent;
                watcher.OnGarbageCollectionEnded += AddStopEvent;
               
                
                var process = AllocationManager.StartProcess(new AllocateOptions { AllocationModeRaw = _options.AllocationModeRaw, Duration = _options.Duration });
                watcher.Start(process.Id, _ticker.Peek());

                var timer = new Timer(state =>
                {
                    _eventsBatchGenerated.Set();
                }, null, (long)_options.Duration.TotalMilliseconds, Timeout.Infinite);

                WriteFile(() => !process.HasExited, false);

                timer.Dispose();
            }
            
            Console.WriteLine();

            // watcher can add events to the queue after writting was completed
            InitQueue();

            StartDataExtrapolation(_options.DesiredEventsCount);
            WriteFile(() => !_extrapolationCompleted, true);

        }
        
        private void StartDataExtrapolation(int desiredEventsCount)
        {
            desiredEventsCount -= _allEvents.Count;
            if (desiredEventsCount <= 0)
            {
                _extrapolationCompleted = true;
                return;
            }

            var thread = new Thread(() =>
            {
                var random = new Random();
                var spinWait = new SpinWait();
                int totalEventsGenerated = 0;
                long lastEventTime = _allEvents[_allEvents.Count - 1].Ticks;
                
                Console.WriteLine("\nExtrapolation started...");

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

                    _eventsBatchGenerated.Set();
                    
                    while (_queue.Count > 1000)
                        spinWait.SpinOnce();
                    
                    spinWait.Reset();
                }

                _extrapolationCompleted = true;
            });
            thread.Start();
        }

        private void AddStartEvent(long ticks)
        {
            var realEvent = new RealEvent(EventType.Start, ticks);
            
            _queue.Enqueue(realEvent);
            _allEvents.Add(realEvent);
        }

        private void AddStopEvent(long ticks)
        {
            var realEvent = new RealEvent(EventType.Stop, ticks);
            
            _queue.Enqueue(realEvent);
            _allEvents.Add(realEvent);
            var count = _queue.Count;
            if (count >= 10)
                _eventsBatchGenerated.Set();
        }

        
        
    }
}
