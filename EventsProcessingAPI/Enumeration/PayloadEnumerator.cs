using EventsDomain;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventsProcessingAPI
{
    public class PayloadEnumerator : AbstractEventEnumerator<Payload>
    {
        public PayloadEnumerator(Memory<Bucket> buckets, int firstEventIndex, int lastEventIndex) 
            : base(buckets, firstEventIndex, lastEventIndex)
        {
        }

        public ref Payload Current => ref _buckets.Span[_currentBucketIndex].Payloads[_currentEventIndex];

        public void Dispose()
        {

        }

        protected override void SetCurrentItem()
        {
        }
    }
}
