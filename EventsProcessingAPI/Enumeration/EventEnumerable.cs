using EventsDomain;
using System;
using System.Collections.Generic;

namespace EventsProcessingAPI
{
    public class EventEnumerable : AbstractEventEnumerable
    {
        public EventEnumerable(Memory<Bucket> buckets, int firstEventIndex, int lastEventIndex) 
            : base(buckets, firstEventIndex, lastEventIndex)
        {
        }

        public EventEnumerator GetEnumerator()
        {
            return new EventEnumerator(_buckets, _firstEventIndex, _lastEventIndex);
        }
        
    }
    
}
