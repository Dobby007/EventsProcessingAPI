using EventsDomain;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventsProcessingAPI
{
    public class EventEnumerator : AbstractEventEnumerator<EventBucketInfo>
    {
        private EventBucketInfo _currentItem;

        public EventEnumerator(Memory<Bucket> buckets, int firstEventIndex, int lastEventIndex) 
            : base(buckets, firstEventIndex, lastEventIndex)
        {
        }

        public ref EventBucketInfo Current => ref _currentItem;

        public void Dispose()
        {

        }

        
        protected override void SetCurrentItem()
        {
            Bucket currentBucket = _buckets.Span[_currentBucketIndex];

            _currentItem = new EventBucketInfo(
                currentBucket.Events[_currentEventIndex],
                _currentBucketIndex,
                _currentEventIndex
            );
        }
    }
}
