using EventsDomain;
using System;
using System.Collections.Generic;

namespace EventsProcessingAPI.Enumeration
{
    public abstract class AbstractEventEnumerable
    {
        protected Memory<Bucket> _buckets;
        protected readonly int _firstEventIndex;
        protected readonly int _lastEventIndex;
        public ReadOnlySpan<Bucket> Buckets => _buckets.Span;

        protected AbstractEventEnumerable(Memory<Bucket> buckets, int firstEventIndex, int lastEventIndex)
        {
            _buckets = buckets;
            _firstEventIndex = firstEventIndex;
            _lastEventIndex = lastEventIndex;
        }

        public int Count 
        {
            get
            {
                var bucketCount = _buckets.Span.Length;
                if (bucketCount == 1)
                    return _lastEventIndex - _firstEventIndex + 1;

                int totalCount = 0;
                
                for (var i = 0; i < bucketCount; i++)
                {
                    if (i == 0)
                        totalCount += _buckets.Span[i].Events.Length - _firstEventIndex;
                    else if (i == bucketCount - 1)
                        totalCount += _lastEventIndex + 1;
                    else
                        totalCount += _buckets.Span[i].Events.Length;
                }

                return totalCount;
            }
        }
        
        
    }
    
}
