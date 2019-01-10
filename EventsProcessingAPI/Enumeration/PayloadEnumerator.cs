using EventsDomain;
using System;

namespace EventsProcessingAPI.Enumeration
{
    public ref struct PayloadEnumerator
    {
        private Span<Bucket> _buckets;
        private readonly int _firstEventIndex;
        private readonly int _lastEventIndex;
        private readonly bool _canMove;
        private int _currentBucketIndex;
        private int _currentEventIndex;

        public PayloadEnumerator(Span<Bucket> buckets, int firstEventIndex, int lastEventIndex)
        {
            _buckets = buckets;
            _firstEventIndex = firstEventIndex;
            _lastEventIndex = lastEventIndex;
            _canMove = _buckets.Length > 0 && !_buckets[0].NoPayloadsLoaded;
            _currentBucketIndex = 0;
            _currentEventIndex = _firstEventIndex - 1;
        }


        public bool MoveNext()
        {
            if (!_canMove)
                return false;
            
            return EnumerationHelper.MoveNext(_buckets, ref _currentBucketIndex, ref _currentEventIndex, _lastEventIndex);
        }


        public void Reset()
        {
            _currentBucketIndex = 0;
            _currentEventIndex = _firstEventIndex - 1;
        }

        public ref Payload Current => ref _buckets[_currentBucketIndex].Payloads[_currentEventIndex];

        public void Dispose()
        {

        }
    }
}
