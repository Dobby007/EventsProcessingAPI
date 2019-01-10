﻿using C5;
using EventsProcessingAPI.Common;
using System;
using System.Linq;
using System.Runtime.CompilerServices;

namespace EventsProcessingAPI.Density
{
    internal class DensityHintContainer
    {
        public const TimeUnit MinHintTimeUnit = TimeUnit.Second;
        public const TimeUnit MaxHintTimeUnit = TimeUnit.Minute;

        // Max period = 49 days
        private readonly TreeDictionary<uint, double> _seconds;

        private readonly TreeDictionary<uint, double> _minutes;

        internal TreeDictionary<uint, double> Seconds => _seconds;
        internal TreeDictionary<uint, double> Minutes => _minutes;

        public DensityHintContainer(
            TreeDictionary<uint, double> seconds, 
            TreeDictionary<uint, double> minutes)
        {
            _seconds = seconds;
            _minutes = minutes;
        }



        /// <summary>
        /// Calculates avegare density for segments of segmentSize length using only precalculated density hints
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="segmentSize"></param>
        /// <returns></returns>
        public bool TrySetDensitiesUsingHints(
            long start, 
            long end, 
            long segmentSize, 
            double[] targetBuffer, 
            out int processedSegmentsCount, 
            out long processedSegmentSize)
        {
            // we can't use hints if segment size is lesser than 1 second
            if (segmentSize < TimeUnitDurations.Second)
            {
                processedSegmentSize = 0;
                processedSegmentsCount = 0;
                return false;
            }

            if (!TryGetMaxDensityHintTimeUnit(segmentSize, out TimeUnit densityHintType, out processedSegmentSize))
            {
                processedSegmentsCount = 0;
                return false;
            }


            long duration = densityHintType.GetTimeUnitDuration();

            ushort hintCount = checked((ushort)(processedSegmentSize / duration));
            uint startInTimeUnits = checked((uint)(start / duration));
            uint endInTimeUnits = checked((uint)(start / duration));

            ushort segmentsCount = checked((ushort)((end - start) / segmentSize));
            for (ushort index = 0; index < segmentsCount; index++)
            {
                targetBuffer[index] = GetAverageDensity(startInTimeUnits + (uint)(index * hintCount) + 1, hintCount, densityHintType);
            }

            processedSegmentsCount = segmentsCount;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private double GetAverageDensity(uint start, ushort hintCount, TimeUnit densityHintType)
        {
            var duration = densityHintType.GetTimeUnitDuration();

            IDirectedEnumerable<C5.KeyValuePair<uint, double>> range;
            switch (densityHintType)
            {
                case TimeUnit.Second:
                    range = _seconds.RangeFromTo(start, start + hintCount);
                    break;
                case TimeUnit.Minute:
                    range = _minutes.RangeFromTo(start, start + hintCount);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(densityHintType));
            }

            return range.Take(hintCount).Sum(kvp => kvp.Value) / hintCount;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool TryGetMaxDensityHintTimeUnit(long segmentSize, out TimeUnit timeUnit, out long maxSegmentSize)
        {
            double floatSegmentSize = segmentSize, hintsCountPerOneSegment = 0;
            if ((hintsCountPerOneSegment = floatSegmentSize / TimeUnitDurations.Minute) >= 1)
            {
                maxSegmentSize = (long)Math.Floor(hintsCountPerOneSegment) * TimeUnitDurations.Minute;
                timeUnit = TimeUnit.Minute;
                return true;
            } 
            else if ((hintsCountPerOneSegment = floatSegmentSize / TimeUnitDurations.Second) >= 1)
            {
                maxSegmentSize = (long)Math.Floor(hintsCountPerOneSegment) * TimeUnitDurations.Second;
                timeUnit = TimeUnit.Second;
                return true;
            }

            maxSegmentSize = default;
            timeUnit = default;
            return false;
        }

        
        
    }
}
