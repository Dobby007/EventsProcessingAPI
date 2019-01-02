using EventsDomain;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventsProcessingAPI
{
    public ref struct EventEnumerator
    {
        private Span<Bucket> _buckets;
        private readonly int _firstEventIndex;
        private readonly int _lastEventIndex;
        private readonly bool _canMove;
        private int _currentBucketIndex;
        private int _currentEventIndex;
        private EventBucketInfo _currentItem;

        public EventEnumerator(Span<Bucket> buckets, int firstEventIndex, int lastEventIndex)
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
            var currentEventIndex = _currentEventIndex;
            var currentBucketIndex = _currentBucketIndex;

            if (!_canMove)
                return false;

            
            if (currentBucketIndex == _buckets.Length - 1 && currentEventIndex + 1 > _lastEventIndex)
                return false;
                
            if (++currentEventIndex < _buckets[currentBucketIndex].Events.Length)
            {
                _currentEventIndex = currentEventIndex;
                SetCurrentItem();
                return true;
            }

            if (++currentBucketIndex < _buckets.Length)
            {
                _currentBucketIndex = currentBucketIndex;
                _currentEventIndex = 0;
                SetCurrentItem();
                return true;
            }

            
            return false;
        }

        private void SetCurrentItem()
        {
            _currentItem = new EventBucketInfo(
                _buckets[_currentBucketIndex].Events[_currentEventIndex],
                _currentBucketIndex,
                _currentEventIndex
            );
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
