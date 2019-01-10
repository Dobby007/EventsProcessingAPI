using EventsDomain;
using System;

namespace EventsProcessingAPI.Enumeration
{
    public class RealEventEnumerable : AbstractEventEnumerable
    {
        public RealEventEnumerable(Memory<Bucket> buckets, int firstEventIndex, int lastEventIndex) 
            : base(buckets, firstEventIndex, lastEventIndex)
        {
        }
        public static RealEventEnumerable Empty { get; } = new RealEventEnumerable(Memory<Bucket>.Empty, 0, 0);

        public RealEventEnumerator GetEnumerator()
        {
            return new RealEventEnumerator(_buckets.Span, _firstEventIndex, _lastEventIndex);
        }
    }
    
}
