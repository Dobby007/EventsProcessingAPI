﻿using EventsDomain;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace RandomDataGenerator
{
    class FakeDataGenerator : DataGenerator
    {
        private Thread _generationThread;

        private readonly uint _maxEventsInterval;
        private readonly uint _maxDuration;
        private readonly int _desiredEventsCount;
        /// <summary>
        /// Generator of the fake events
        /// </summary>
        /// <param name="filename">Path to the generated file</param>
        /// <param name="desiredEventsCount">Desired number of events in the generated file</param>
        /// <param name="maxEventsInterval">Max interval between stop and start events</param>
        /// <param name="maxDuration">Max interval between start and stop events</param>
        public FakeDataGenerator(string filename, int desiredEventsCount, uint maxEventsInterval = 10_000, uint maxDuration = 1_000)
            : base(filename)
        {
            _desiredEventsCount = desiredEventsCount;
            _maxEventsInterval = maxEventsInterval;
            _maxDuration = maxDuration;
        }

        public void GenerateFile()
        {
            StartFakeDataGeneration(_desiredEventsCount);
            WriteFile(() => _generationThread.IsAlive, false);
        }
        
        private void StartFakeDataGeneration(int desiredEventsCount)
        {
            var timestamp = _ticker.Peek();
            _generationThread = new Thread(() => 
            {
                var timeRandomizer = new Random();
                for (var i = 0; i <= desiredEventsCount; i = i + 2)
                {
                    timestamp += (long)(timeRandomizer.NextDouble() * _maxEventsInterval) + 1;
                    AddStartEvent(timestamp);

                    timestamp += (long)(timeRandomizer.NextDouble() * _maxDuration) + 1;
                    AddStopEvent(timestamp);
                }
            });
            _generationThread.IsBackground = true;
            _generationThread.Start();
        }

        private void AddStartEvent(long ticks)
        {
            var realEvent = new RealEvent(EventType.Start, ticks);
            _queue.Enqueue(realEvent);
        }

        private void AddStopEvent(long ticks)
        {
            var realEvent = new RealEvent(EventType.Stop, ticks);
            
            _queue.Enqueue(realEvent);
            if (_queue.Count >= 10)
                _eventsBatchGenerated.Set();
        }

        
    }
}
