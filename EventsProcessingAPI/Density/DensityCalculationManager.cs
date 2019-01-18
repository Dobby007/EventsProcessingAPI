using EventsDomain;
using EventsProcessingAPI.Enumeration;
using EventsProcessingAPI.Exceptions;
using EventsProcessingAPI.Ranges;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace EventsProcessingAPI.Density
{
    internal static class DensityCalculationManager
    {
        const int ParallelBatchSize = 100;

        /// <summary>
        /// Gets densities for segments with size equal to <paramref name="segmentSize"/>
        /// </summary>
        /// <param name="container">Bucket container</param>
        /// <param name="start">Start time</param>
        /// <param name="end">End time</param>
        /// <param name="segmentSize">Length/duration of one segment</param>
        public static double[] GetDensities(BucketContainer container, long start, long end, long segmentSize)
        {
            double[] densities = null;
            GetDensities(container, start, end, segmentSize, targetBuffer: ref densities);
            return densities ?? Array.Empty<double>();
        }

        /// <summary>
        /// Gets densities for segments with size equal to <paramref name="segmentSize"/>
        /// </summary>
        /// <param name="container">Bucket container</param>
        /// <param name="start">Start time</param>
        /// <param name="end">End time</param>
        /// <param name="segmentSize">Length/duration of one segment</param>
        /// <param name="targetBuffer">Targer buffer into  which calculated densities are saved</param>
        /// <returns></returns>
        public static Span<double> GetDensities(BucketContainer container, long start, long end, long segmentSize, ref double[] targetBuffer)
        {
            if (end <= start)
            {
                throw new ArgumentException("Wrong interval. End timestamp must be greater than start timestamp.");
            }

            if (end - start < segmentSize)
            {
                throw new ArgumentException("Segment size is too big for this time interval", nameof(segmentSize));
            }


            var lastBucket = container.GetLastBucket();
            var maxTime = lastBucket.GetAbsoluteTimeForEvent(lastBucket.GetLastEvent());
            if (end > maxTime + 1)
            {
                end = maxTime + 1;
            }

            // Start time is out of range
            if (end < start)
                return Span<double>.Empty;

            ushort totalSegments = GetTotalSegments(start, end, segmentSize);

            if (targetBuffer == null)
                targetBuffer = new double[totalSegments];
            else if (targetBuffer.Length < totalSegments)
                throw new ArgumentException("Target buffer is too short", nameof(targetBuffer));

            int processedSegmentsUsingHints = -1;
            try
            {
                checked
                {
                    container.DensityHintContainer?.TrySetDensitiesUsingHints(
                        start - container.FirstTimestamp,
                        end - container.FirstTimestamp,
                        segmentSize,
                        targetBuffer,
                        out processedSegmentsUsingHints
                    );
                }
            }
            catch (OverflowException)
            {
                throw new InvalidOperationException("Too big range of events");
            }

#if DEBUG
            if (processedSegmentsUsingHints == 0 && segmentSize >= 10_000_000)
                Debug.Fail("Density calculation is perfoming in unoptimized way. It can be eliminated by adjusting range and segment size to appropriate values.");
#endif

            if (processedSegmentsUsingHints < 0)
                processedSegmentsUsingHints = 0;

            if (processedSegmentsUsingHints == totalSegments)
                return targetBuffer.AsSpan(0, totalSegments);

            start += processedSegmentsUsingHints * segmentSize;
            int uncalculatedDensitiesCount = totalSegments - processedSegmentsUsingHints;

            var targetBufferCopy = targetBuffer;
            Parallel.ForEach(GetPartitions(start, segmentSize, targetBuffer.AsSpan(processedSegmentsUsingHints, uncalculatedDensitiesCount)), (partition) =>
            {
                Span<double> batch = targetBufferCopy.AsSpan(processedSegmentsUsingHints, uncalculatedDensitiesCount)
                    .Slice(partition.leftBoundary, partition.batchLength);

                DensityCalculator.CalculateDensities(
                    container.Buckets,
                    new DensityCalculationRequest(partition.start, partition.end, segmentSize),
                    batch,
                    true,
                    out long processedRange
                );
            });


            return targetBuffer.AsSpan(0, totalSegments);
        }

        /// <summary>
        /// Gets densities for segments with size equal to <paramref name="segmentSize"/>
        /// </summary>
        /// <param name="buckets">Array of buckets</param>
        /// <param name="start">Start timestamp of the event range we want to find. If null, then it is a first event time in the first bucket of the buckets array.</param>
        /// <param name="segmentSize">Length/duration of one segment</param>
        /// <param name="finalize">Flag indicating that density should be calculated for the last incomplete segment if there is one</param>
        /// <param name="nextBatchStartTime">Next start time</param>
        /// <returns></returns>
        public static double[] GetDensities(ArraySegment<Bucket> bucketsArray, long? start, long segmentSize, bool finalize, out long nextBatchStartTime)
        {
            var buckets = bucketsArray.AsSpan();
            if (buckets.Length < 1)
            {
                throw new ArgumentException("Empty array of buckets is not allowed");
            }

            if (!start.HasValue)
                start = buckets[0].GetAbsoluteTimeForEvent(buckets[0].GetFirstEvent());

            long end = buckets[buckets.Length - 1].GetAbsoluteTimeForEvent(buckets[buckets.Length - 1].GetLastEvent()) + 1;

            ushort totalSegments = 0;
            try
            {
                if (finalize)
                    totalSegments = checked((ushort)Math.Ceiling((end - start.Value) / (double)segmentSize));
                else
                    totalSegments = checked((ushort)Math.Floor((end - start.Value) / (double)segmentSize));
            }
            catch (OverflowException)
            {
                throw new ArgumentException("Too small segment size for such a big range", nameof(segmentSize));
            }

            var densitiesBuf = new double[totalSegments];
            DensityCalculator.CalculateDensities(
                bucketsArray.AsMemory(),
                new DensityCalculationRequest(start.Value, end, segmentSize),
                densitiesBuf,
                finalize,
                out long processedRange
            );

            nextBatchStartTime = start.Value + processedRange;

            return densitiesBuf;
        }


        private static IList<(long start, long end, int leftBoundary, int batchLength)> GetPartitions(long start, long segmentSize, Span<double> targetBuffer)
        {
            int partitionCount = (int)Math.Ceiling(targetBuffer.Length / (double)ParallelBatchSize);
            var list = new List<(long, long, int, int)>(partitionCount);

            for (var partitionIndex = 0; partitionIndex < partitionCount; partitionIndex++)
            {
                long partitionStartTime, partitionEndTime;
                int leftBoundary = partitionIndex * ParallelBatchSize;
                if (leftBoundary + ParallelBatchSize >= targetBuffer.Length)
                {
                    int batchLength = targetBuffer.Length - partitionIndex * ParallelBatchSize;
                    partitionStartTime = start + leftBoundary * segmentSize;
                    partitionEndTime = start + (leftBoundary + batchLength) * segmentSize;
                    list.Add((partitionStartTime, partitionEndTime, leftBoundary, batchLength));
                }
                else
                {
                    partitionStartTime = start + leftBoundary * segmentSize;
                    partitionEndTime = start + (leftBoundary + ParallelBatchSize) * segmentSize - 1;
                    list.Add((partitionStartTime, partitionEndTime, leftBoundary, ParallelBatchSize));
                }
            }

            return list;
        }

        private static ushort GetTotalSegments(long start, long end, double segmentSize)
        {
            ushort totalSegments = 0;
            try
            {
                totalSegments = checked((ushort)Math.Ceiling((end - start) / segmentSize));
            }
            catch (OverflowException)
            {
                throw new ArgumentException("Too small segment size for such a big range", nameof(segmentSize));
            }
            return totalSegments;
        }
    }
}
