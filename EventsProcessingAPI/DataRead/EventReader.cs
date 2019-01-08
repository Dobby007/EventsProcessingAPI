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
using EventsProcessingAPI.DataRead;

namespace EventsProcessingAPI.DataRead
{
    internal class EventReader : IEventReader
    {
        private readonly BinaryReader _reader;
        private BlockingCollection<Bucket> _buckets = new BlockingCollection<Bucket>();
        public long FirstTimestamp { get; private set; }
        private readonly IBucketBuilder _builder;
        private const int RealEventSize = 9;
        private const int RealPayloadSize = 32;


        public EventReader(Stream stream, IBucketBuilder builder)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            _reader = new BinaryReader(stream);
            _builder = builder;
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
            long firstTimestamp = -1;
            double? totalStreamLength = null;
            long lastEventTime = 0;
            long eventsCountRead = 0;
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
                    if (firstTimestamp < 0)
                    {
                        // We manipulate with microseconds
                        firstTimestamp = FirstTimestamp = eventTime;
                        _builder.StartNewBucket(eventTime);
                    }

                    // We need to create a new bucket if an event does not fit into the previous one
                    if (!_builder.IsFitIntoCurrentBucket(eventTime))
                    {
                        if (totalStreamLength.HasValue)
                        {
                            progress.Report((int)(_reader.BaseStream.Position / totalStreamLength.Value * 100));
                        }

                        _buckets.Add(_builder.Build(enablePayload));
                        _builder.StartNewBucket(eventTime);
                    }
                   
                    long first = _reader.ReadInt64();
                    long second = _reader.ReadInt64();
                    long third = _reader.ReadInt64();
                    long fourth = _reader.ReadInt64();

                    EventType eventType = isStartEvent ? EventType.Start : EventType.Stop;
                    if (lastEventType == eventType)
                        continue;

                    // We don't need payloads if user does not want to know anything about them
                    if (enablePayload)
                        _builder.AddEvent(new RealEvent(eventType, eventTime));
                    else
                        _builder.AddEventWithPayload(new RealEvent(eventType, eventTime), new Payload(first, second, third, fourth));
                    
                    lastEventTime = eventTime;
                    lastEventType = eventType;
                    eventsCountRead++;
                }
            }
            catch (EndOfStreamException)
            {

                var lastBucket = _builder.Build(enablePayload);
                if (lastBucket != null)
                    _buckets.Add(lastBucket);

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
