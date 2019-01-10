using EventsDomain;
using System;
using System.Collections.Generic;

namespace EventsProcessingAPI.Enumeration
{
    public class EventEnumerable : AbstractEventEnumerable
    {
        public EventEnumerable(Memory<Bucket> buckets, int firstEventIndex, int lastEventIndex) 
            : base(buckets, firstEventIndex, lastEventIndex)
        {
        }
        public static EventEnumerable Empty { get; } = new EventEnumerable(Memory<Bucket>.Empty, 0, 0);

        public EventEnumerator GetEnumerator()
        {
            return new EventEnumerator(_buckets.Span, _firstEventIndex, _lastEventIndex);
        }
    }
    
}
