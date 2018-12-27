using EventsDomain;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Threading;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace SimpleTest
{
    public class EventReader : IDisposable
    {
        private readonly BinaryReader _reader;
        private BlockingCollection<RealEvent> _events = new BlockingCollection<RealEvent>();
        public long FirstTimestamp { get; private set; }

        public EventReader(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            _reader = new BinaryReader(stream);
        }

        public uint TotalEventsCount { get; private set; }

        public IEnumerable<RealEvent> GetAllEvents()
        {
            return _events.GetConsumingEnumerable();
        }


        public Task StartReadingEvents(bool enablePayload, IProgress<int> progress)
        {
            var tcs = new TaskCompletionSource<bool>();

            var producerThread = new Thread(() =>
            {
                try
                {
                    ReadAllEvents(enablePayload, progress);
                    tcs.SetResult(true);
                }
                catch (Exception exc)
                {
                    tcs.SetException(exc);
                }
            });
            producerThread.Start();

            return tcs.Task;
        }

        private void ReadAllEvents(bool enablePayload, IProgress<int> progress)
        {
            
            
            var eventsBuffer = new Event[ushort.MaxValue];
            long? firstTimestamp = null;
            double? totalStreamLength = null;
            long lastEventTime = 0;

            if (GetEventsCount(out var eventsCount, out var streamLength))
            {
                TotalEventsCount = eventsCount;
                totalStreamLength = streamLength;
            }

            try
            {
                while (true)
                {
                    bool isStartEvent = !_reader.ReadBoolean();
                    long eventTime = _reader.ReadInt64() / 10;

                    Debug.Assert(eventTime > lastEventTime);

                    if (!firstTimestamp.HasValue)
                    {
                        firstTimestamp = FirstTimestamp = eventTime;
                    }
                    

                    _events.Add(new RealEvent(isStartEvent ? EventType.Start : EventType.Stop, eventTime));

                    _reader.ReadInt64();
                    _reader.ReadInt64();
                    _reader.ReadInt64();
                    _reader.ReadInt64();
                    
                    lastEventTime = eventTime;
                }
            }
            catch (EndOfStreamException)
            {
                progress.Report(100);
            }
            finally
            {
                _events.CompleteAdding();
            }
        }
        

        private bool GetEventsCount(out uint eventsCount, out long totalStreamLength)
        {
            try
            {
                totalStreamLength = _reader.BaseStream.Length;
                RealEvent ev = new RealEvent();
                Payload payload = new Payload();
                eventsCount = (uint)(totalStreamLength / 
                    (Marshal.SizeOf(ev) + Marshal.SizeOf(payload)));
                
                return true;
            }
            catch (NotSupportedException)
            {
                eventsCount = default;
                totalStreamLength = default;
                return false;
            }
        }

        #region IDisposable Support
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _reader?.Dispose();
                }
                
                disposedValue = true;
            }
        }
        
        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
