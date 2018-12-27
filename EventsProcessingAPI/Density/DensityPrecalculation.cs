using C5;
using EventsDomain;
using EventsProcessingAPI.Common;
using EventsProcessingAPI.Common.Pipeline;
using System;
using System.Runtime.CompilerServices;
using static EventsProcessingAPI.Ranges.RangeSelector;

namespace EventsProcessingAPI.Density
{
    class DensityPrecalculation : IProcess
    {
        private readonly TreeDictionary<uint, double> _densityHintsForSeconds;
        private readonly TreeDictionary<uint, double> _densityHintsForMinutes;
        
        private long? _lastProcessedAbsoluteTime1;
        private long? _lastProcessedAbsoluteTime2;
        private bool _isCompleted = false;

        public DensityPrecalculation()
        {
            _densityHintsForSeconds = new TreeDictionary<uint, double>();
            _densityHintsForMinutes = new TreeDictionary<uint, double>();
        }

        public void Complete(BucketContainer container)
        {
            if (_isCompleted)
                return;

            ProcessNewBuckets(new ArraySegment<Bucket>(container.Buckets), true);
            container.DensityHintContainer = new DensityHintContainer(
                _densityHintsForSeconds,
                _densityHintsForMinutes
            );

            _isCompleted = true;
        }

        public void ProcessBucket(ArraySegment<Bucket> buckets, int index)
        {
            if (index % 500 == 0)
                ProcessNewBuckets(buckets, false);
        }

        private void ProcessNewBuckets(ArraySegment<Bucket> bucketsArray, bool isCompleted)
        {
            var buckets = bucketsArray.AsSpan();
            var lastBucket = buckets[buckets.Length - 1];
            var lastEventAbsoluteTime = lastBucket.GetAbsoluteTimeForEvent(lastBucket.GetLastEvent());
            var firstEventAbsoluteTime = buckets[0].GetAbsoluteTimeForEvent(buckets[0].GetFirstEvent());

            uint segmentSize = 1_000_000;

            if (isCompleted || lastEventAbsoluteTime - (GetStartTime(buckets, TimeUnit.Second) ?? firstEventAbsoluteTime) > segmentSize * 30)
            {
                var densities = DensityCalculator.GetDensities(
                    bucketsArray,
                    GetStartTime(buckets, TimeUnit.Second),
                    segmentSize,
                    isCompleted,
                    out Range processedRange);

                _lastProcessedAbsoluteTime1 = buckets[processedRange.LastBucketIndex].GetAbsoluteTimeForEvent(processedRange.LastEventIndex);
                AddDensityHints(densities, TimeUnit.Second, segmentSize);
            }

            segmentSize *= 60;
            if (isCompleted || lastEventAbsoluteTime - (GetStartTime(buckets, TimeUnit.Minute) ?? firstEventAbsoluteTime) > segmentSize)
            {
                var densities = DensityCalculator.GetDensities(
                    bucketsArray,
                    GetStartTime(buckets, TimeUnit.Minute),
                    segmentSize,
                    isCompleted,
                    out Range processedRange);

                _lastProcessedAbsoluteTime2 = buckets[processedRange.LastBucketIndex].GetAbsoluteTimeForEvent(processedRange.LastEventIndex);
                AddDensityHints(densities, TimeUnit.Minute, segmentSize);

            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AddDensityHints(double[] densities, TimeUnit timeUnit, uint segmentSize)
        {
            var targetDictionary = timeUnit == TimeUnit.Second ? _densityHintsForSeconds : _densityHintsForMinutes;
            var timeOffset = !targetDictionary.IsEmpty ? targetDictionary.FindMax().Key : 0;

            for (int i = 1; i <= densities.Length; i++)
            {
                if (densities[i - 1] > 0)
                    targetDictionary.Add(checked(timeOffset + (uint)i), densities[i - 1]);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private long? GetStartTime(ReadOnlySpan<Bucket> buckets, TimeUnit timeUnit)
        {
            switch (timeUnit)
            {
                case TimeUnit.Second:
                    return _lastProcessedAbsoluteTime1.HasValue ? _lastProcessedAbsoluteTime1 + 1 : null;

                case TimeUnit.Minute:
                    return _lastProcessedAbsoluteTime2.HasValue ? _lastProcessedAbsoluteTime2 + 1 : null;

                default:
                    throw new ArgumentOutOfRangeException(nameof(timeUnit));
            }
        }
    }
}
