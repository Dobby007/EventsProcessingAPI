using EventsDomain;
using EventsProcessingAPI.Enumeration;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventsProcessingAPI.Enumeration
{
    public ref struct PartitionedEnumerator
    {
        private Span<Bucket> _buckets;
        private readonly int _firstEventIndex;
        private readonly int _lastEventIndex;
        private readonly bool _canMove;
        private int _currentBucketIndex;
        private int _currentEventIndex;
        private long _partitionSize;
        private long _partitionPadding;
        private EventBucketInfo _currentItem;

        public PartitionedEnumerator(Span<Bucket> buckets, int firstEventIndex, int lastEventIndex, long partitionSize, long partitionPadding)
        {
            _buckets = buckets;
            _firstEventIndex = firstEventIndex;
            _lastEventIndex = lastEventIndex;
            _canMove = _buckets.Length > 0;
            _currentBucketIndex = 0;
            _currentEventIndex = _firstEventIndex - 1;
            _currentItem = default;
        }


        public bool MoveNext()
        {
            if (!_canMove)
                return false;


            int currentBucketIndex = _currentBucketIndex;
            int currentEventIndex = _currentEventIndex;

            if (currentBucketIndex == _buckets.Length - 1 && currentEventIndex + 1 > _lastEventIndex)
                return false;

            bool moveResult = false;

            // todo: is first move
            Bucket currentBucket = _buckets[currentBucketIndex];
            if (++currentEventIndex < currentBucket.Events.Length)
            {
                _currentEventIndex = currentEventIndex;
                moveResult = true;
            }

            

            if (++currentBucketIndex < _buckets.Length)
            {
                currentEventIndex = 0;
                moveResult = true;
            }

            if (moveResult)
            {
                if (currentBucketIndex == _currentBucketIndex)
                {
                    long currentBucketTime = _buckets[currentBucketIndex].StartTime;
                    if (currentBucketTime + currentBucket.Events[currentEventIndex].RelativeTime > )
                }

                Bucket newBucket = _buckets[currentBucketIndex];
                if (newBucket.GetAbsoluteTimeForEvent(currentEventIndex) - currentBucket.GetAbsoluteTimeForEvent(_currentEventIndex))
                {

                }
            }

            return moveResult;
            
            
        }


        public void Reset()
        {
            _currentBucketIndex = 0;
            _currentEventIndex = _firstEventIndex - 1;
        }

        public EventBucketInfo Current => _currentItem;

        public void Dispose()
        {

        }
    }
}
