using EventsDomain;
using EventsProcessingAPI;
using System;
using System.Diagnostics;
using System.Linq;

namespace PerfomanceTest
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Usage: dotnet PerfomanceTest.dll {pathToFile}");
                Console.WriteLine("Example: dotnet PerfomanceTest.dll ..\\..\\..\\..\\data\\events.bin");
                return;
            }

            var eventsApi = new ApiFacade
            {
                ProgressHandler = new ConsoleProgress()
            };
            
            var container = eventsApi.LoadEventsFromFileAsync(args[0], LoadStrategyType.LoadEventsForChart)
                .GetAwaiter()
                .GetResult();

            Console.WriteLine();

            Bucket lastBucket = container.GetLastBucket();
            long startTimestamp = container.FirstTimestamp,
                 endTimestamp = lastBucket?.GetAbsoluteTimeForEvent(lastBucket.GetLastEvent()) ?? 0;

            
            RunPerfomanceTests(
                container, 
                startTimestamp, 
                endTimestamp, 
                container.GetPreferredSegmentSizes(startTimestamp, endTimestamp, 800)
            );
        }

        private static void RunPerfomanceTests(BucketContainer container, long start, long end, long[] segmentSizes)
        {
            bool isWarmUp = true;
            int tries = 0;

            while (tries < 2)
            {
                var stopWatch = new Stopwatch();
                foreach (var segmentSize in segmentSizes)
                {
                    long tStart = start, tEnd = Math.Min(start + segmentSize * 2000, end);
                    stopWatch.Start();
                    var densities = container.GetDensities(tStart, tEnd, segmentSize);
                    var elapsed = stopWatch.ElapsedMilliseconds;
                    stopWatch.Reset();

                    if (!isWarmUp)
                        Console.WriteLine("Segment size {0,10}: {1,4}ms elapsed for the range with length {2}μs: average density = {3}",
                           segmentSize, elapsed, tEnd - tStart, densities.Average());
                }
                tries++;
                isWarmUp = false;
            }
        }
    }
}
