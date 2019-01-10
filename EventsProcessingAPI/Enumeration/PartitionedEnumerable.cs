using EventsDomain;
using System;
using System.Collections.Generic;

namespace EventsProcessingAPI.Enumeration
{
    public class PartitionedEnumerable
    {
        protected Memory<Bucket> _buckets;
        protected readonly int _firstEventIndex;
        protected readonly int _lastEventIndex;

        public PartitionedEnumerable(Memory<Bucket> buckets, int firstEventIndex, int lastEventIndex) 
        {
        }

        public PartitionedEnumerator GetEnumerator()
        {
            return new PartitionedEnumerator(_buckets.Span, _firstEventIndex, _lastEventIndex);
        }
        
    }
    
}
