using EventsDomain;
using System;
using System.Collections.Generic;

namespace EventsProcessingAPI.Enumeration
{
    public class PayloadEnumerable : AbstractEventEnumerable
    {
        public PayloadEnumerable(Memory<Bucket> buckets, int firstEventIndex, int lastEventIndex) 
            : base(buckets, firstEventIndex, lastEventIndex)
        {
        }

        public PayloadEnumerator GetEnumerator()
        {
            return new PayloadEnumerator(_buckets.Span, _firstEventIndex, _lastEventIndex);
        }
        
    }
    
}
