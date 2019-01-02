using EventsDomain;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Threading;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using EventsProcessingAPI.Exceptions;
using System.Diagnostics;

namespace EventsProcessingAPI
{
    public class EventReader : IEventReader
    {
        private readonly BinaryReader _reader;
        private BlockingCollection<Bucket> _buckets = new BlockingCollection<Bucket>();
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

        public IEnumerable<Bucket> GetAllBuckets()
        {
            return _buckets.GetConsumingEnumerable();
        }


        public Task StartReadingEvents(bool enablePayload, IProgress<int> progress)
        {
            var tcs = new TaskCompletionSource<bool>();

            var producerThread = new Thread(() =>
            {
                // TODO: Рассмотреть возможность установки специального флага для IO-потока
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
            // Pre-allocated buffers for events and payloads
            var eventsBuffer = new Event[Bucket.MaxBucketEventTime];
            var payloadsBuffer = new Payload[Bucket.MaxBucketEventTime];

            int eventsBucketSize = 0;
            long? firstTimestamp = null;
            long bucketOffset = 0;
            double? totalStreamLength = null;
            long lastEventTime = 0;
            EventType lastEventType = EventType.Stop; // first event type is Start anyway

            // Try to calculate total events count
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
                    long eventTime = _reader.ReadInt64();

                    // The current event time must be greater than the previous one, othewise a file is corrupted
                    if (eventTime < lastEventTime)
                        throw new BadEventSourceException("File is corrupted. Events are not sorted in ascending order.");
                    
                    // The following condition is true only for the first iteration
                    if (!firstTimestamp.HasValue)
                    {
                        // We manipulate with microseconds
                        firstTimestamp = FirstTimestamp = eventTime;
                        bucketOffset = eventTime / Bucket.MaxBucketEventTime;
                    }

                    // We need to create a new bucket if an event does not fit into the previous one
                    if (eventTime / Bucket.MaxBucketEventTime != bucketOffset)
                    {
                        if (totalStreamLength.HasValue)
                        {
                            progress.Report((int)(_reader.BaseStream.Position / totalStreamLength.Value * 100));
                        }

                        _buckets.Add(GetBucket(eventsBuffer, enablePayload ? payloadsBuffer : null, eventsBucketSize, bucketOffset));
                        eventsBucketSize = 0;
                        bucketOffset = eventTime / Bucket.MaxBucketEventTime;
                    }
                    

                    uint bucketEventTime = Convert.ToUInt32(eventTime % Bucket.MaxBucketEventTime);

                    long first = _reader.ReadInt64();
                    long second = _reader.ReadInt64();
                    long third = _reader.ReadInt64();
                    long fourth = _reader.ReadInt64();

                    EventType eventType = isStartEvent ? EventType.Start : EventType.Stop;
                    if (lastEventType == eventType)
                        continue;

                    eventsBuffer[eventsBucketSize] = new Event(eventType, bucketEventTime);
                    // We don't need payloads if user does not want to know anything about them
                    if (enablePayload)
                    {
                        payloadsBuffer[eventsBucketSize] = new Payload(first, second, third, fourth);
                    }

                    #if DEBUG
                    var curEvent = eventsBuffer[eventsBucketSize];
                    Debug.Assert(
                        eventsBucketSize < 1 ||
                        curEvent.EventTimeHigh > eventsBuffer[eventsBucketSize - 1].EventTimeHigh ||
                        (curEvent.EventTimeHigh == eventsBuffer[eventsBucketSize - 1].EventTimeHigh && curEvent.EventTimeLow > eventsBuffer[eventsBucketSize - 1].EventTimeLow)
                    );
                    #endif

                    eventsBucketSize++;
                    lastEventTime = eventTime;
                    lastEventType = eventType;
                }
            }
            catch (EndOfStreamException)
            {
                if (eventsBuffer.Length > 0)
                    _buckets.Add(GetBucket(eventsBuffer, enablePayload ? payloadsBuffer : null, eventsBucketSize, bucketOffset));
                progress.Report(100);
            }
            finally
            {
                _buckets.CompleteAdding();
            }
        }

        private Bucket GetBucket(Event[] eventsBuffer, Payload[] payloadsBuffer, int bucketSize, long bucketOffset)
        {
            var eventsInBucket = new Event[bucketSize];
            Array.Copy(eventsBuffer, eventsInBucket, bucketSize);
            Payload[] payloadsInBucket = null;

            if (payloadsBuffer != null)
            {
                payloadsInBucket = new Payload[bucketSize];
                Array.Copy(payloadsBuffer, payloadsInBucket, bucketSize);
            }

            return new Bucket(bucketOffset, eventsInBucket, payloadsInBucket);
        }

        /// <summary>
        /// Tries to calculate total events based on the stream length
        /// </summary>
        /// <param name="eventsCount">Total events count</param>
        /// <param name="totalStreamLength">Input stream length</param>
        /// <returns>True if calculation was succeeded</returns>
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
