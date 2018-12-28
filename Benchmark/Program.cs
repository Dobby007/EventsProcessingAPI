using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Validators;
using EventsProcessingAPI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
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

            Environment.SetEnvironmentVariable(DensityCalculationTest.FilePathEnvVariable, args[0]);

            var summary = BenchmarkRunner.Run<DensityCalculationTest>();
        }
    }

    [SimpleJob(launchCount: 1, warmupCount: 1)]
    [MinColumn, MaxColumn, MeanColumn, MedianColumn]
    public class DensityCalculationTest
    {
        public const string FilePathEnvVariable = nameof(DensityCalculationTest) + "_" + "FilePath";
        
        public long Start { get; set; }

        public long End { get; set; }
        
        [Params(1, 500, 1000, 10000, 50000, 100000, 250000, 500000, 1000000, 2000000)]
        public long SegmentSize { get; set; }

        public BucketContainer BucketContainer { get; set; }

        [GlobalSetup]
        public void GlobalSetup()
        {
            try
            {
                var eventsApi = new ApiFacade
                {
                    ProgressHandler = new ConsoleProgress()
                };

                var filePath = Environment.GetEnvironmentVariable(FilePathEnvVariable);
                BucketContainer = eventsApi.LoadEventsFromFileAsync(filePath, LoadStrategyType.LoadEventsForChart)
                    .GetAwaiter()
                    .GetResult();

                var lastBucket = BucketContainer.GetLastBucket();
                Start = BucketContainer.FirstTimestamp;
                End = lastBucket?.GetAbsoluteTimeForEvent(lastBucket.GetLastEvent()) ?? 0;

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
            BucketContainer.GetDensities(tStart, tEnd, SegmentSize);
            
        }
    }
    

}
