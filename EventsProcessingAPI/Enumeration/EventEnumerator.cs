using EventsDomain;
using EventsProcessingAPI.Enumeration;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventsProcessingAPI.Enumeration
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
            if (!_canMove)
                return false;

            var result = EnumerationHelper.MoveNext(_buckets, ref _currentBucketIndex, ref _currentEventIndex, _lastEventIndex);
            if (result)
            {
                _currentItem = new EventBucketInfo(
                    _buckets[_currentBucketIndex].Events[_currentEventIndex],
                    _currentBucketIndex,
                    _currentEventIndex
                );
            }
            
            return result;
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
