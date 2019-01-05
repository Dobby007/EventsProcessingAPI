using EventsDomain;
using EventsProcessingAPI.Enumeration;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventsProcessingAPI
{
    public ref struct RealEventEnumerator
    {
        private Span<Bucket> _buckets;
        private readonly int _firstEventIndex;
        private readonly int _lastEventIndex;
        private readonly bool _canMove;
        private int _currentBucketIndex;
        private int _currentEventIndex;
        private RealEvent _currentItem;

        public RealEventEnumerator(Span<Bucket> buckets, int firstEventIndex, int lastEventIndex)
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
                var bucket = _buckets[_currentBucketIndex];
                var ev = bucket.Events[_currentEventIndex];
                _currentItem = new RealEvent(
                    ev.EventType,
                    bucket.GetAbsoluteTimeForEvent(ev)
                );
            }
            
            return result;
        }


        private void SetCurrentItem()
        {
            
        }


        public void Reset()
        {
            _currentBucketIndex = 0;
            _currentEventIndex = _firstEventIndex - 1;
        }

        public RealEvent Current => _currentItem;

        public void Dispose()
        {

        }
    }
}
