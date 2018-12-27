using EventsDomain;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventsProcessingAPI
{
    public abstract class AbstractEventEnumerator<T>
    {
        protected Memory<Bucket> _buckets;
        protected readonly int _firstEventIndex;
        protected readonly int _lastEventIndex;

        protected int _currentBucketIndex;
        protected int _currentEventIndex;

        public AbstractEventEnumerator(Memory<Bucket> buckets, int firstEventIndex, int lastEventIndex)
        {
            _buckets = buckets;
            _firstEventIndex = firstEventIndex;
            _lastEventIndex = lastEventIndex;
            Reset();
        }
        

        public bool MoveNext()
        {
            var currentEventIndex = _currentEventIndex;
            var currentBucketIndex = _currentBucketIndex;

            if (_buckets.Span.Length < 1)
                return false;

            if (currentBucketIndex == _buckets.Span.Length - 1 && currentEventIndex + 1 > _lastEventIndex)
                return false;


            if (++currentEventIndex < _buckets.Span[currentBucketIndex].Events.Length)
            {
                _currentEventIndex = currentEventIndex;
                SetCurrentItem();
                return true;
            }

            if (++currentBucketIndex < _buckets.Span.Length)
            {
                _currentBucketIndex = currentBucketIndex;
                _currentEventIndex = 0;
                SetCurrentItem();
                return true;
            }

            return false;
        }

        protected abstract void SetCurrentItem();

        

        public void Reset()
        {
            _currentBucketIndex = 0;
            _currentEventIndex = _firstEventIndex - 1;
        }
    }
}
