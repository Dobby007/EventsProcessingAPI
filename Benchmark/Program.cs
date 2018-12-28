using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using EventsProcessingAPI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace BenchmarkTest
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Usage: dotnet BenchmarkTest.dll -- {pathToGeneratedFile}");
                Console.WriteLine("Example: dotnet BenchmarkTest.dll -- ..\\..\\..\\..\\data\\events.bin");
                return;
            }

            DensityCalculationTest.FilePath = args[0];
            /*var test = new DensityCalculationTest();
            test.GlobalSetup();
            test.CalculateDensities();*/

            var summary = BenchmarkRunner.Run<DensityCalculationTest>();
            
            //Console.WriteLine(summary.Reports[0].ToString());
        }
    }

    [SimpleJob(launchCount: 3, warmupCount: 2, targetCount: 30)]
    public class DensityCalculationTest
    {
        public long Start { get; set; }

        public long End { get; set; }

        [ParamsSource(nameof(PreferredSegmentSizes))]
        public long SegmentSize { get; set; }

        public IEnumerable<long> PreferredSegmentSizes { get; set; } = new long[0];

        public BucketContainer BucketContainer { get; set; }

        public static string FilePath { get; set; }

        [GlobalSetup]
        public void GlobalSetup()
        {
            try
            {
                var eventsApi = new ApiFacade
                {
                    ProgressHandler = new ConsoleProgress()
                };

                BucketContainer = eventsApi.LoadEventsFromFileAsync(FilePath, LoadStrategyType.LoadEventsForChart)
                    .GetAwaiter()
                    .GetResult();

                var lastBucket = BucketContainer.GetLastBucket();
                Start = BucketContainer.FirstTimestamp;
                End = lastBucket?.GetAbsoluteTimeForEvent(lastBucket.GetLastEvent()) ?? 0;

                PreferredSegmentSizes = BucketContainer.GetPreferredSegmentSizes(Start, End, 784);

            }
            catch (Exception exc)
            {
                Console.WriteLine();
                Console.WriteLine(exc.ToString());
            }
}

        [Benchmark]
        public void CalculateDensities()
        {
            long tStart = Start, tEnd = Math.Min(Start + SegmentSize * 2000, End);
            //BucketContainer.GetDensities(tStart, tEnd, SegmentSize);
            
        }
    }
    
}
