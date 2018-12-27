using System;
using System.Collections.Generic;
using System.Text;

namespace EventsDomain
{
    public readonly struct BucketEvent
    {
        public readonly Event Event;
        public readonly int BucketIndex;
        public readonly int EventIndex;

        public BucketEvent(Event @event, int bucketIndex, int eventIndex)
        {
            Event = @event;
            BucketIndex = bucketIndex;
            EventIndex = eventIndex;
        }
    }
}
