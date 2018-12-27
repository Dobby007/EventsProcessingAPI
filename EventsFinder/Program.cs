using EventsDomain;
using EventsProcessingAPI;
using EventsProcessingAPI.Density;
using System;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;

namespace EventsUtility
{
    class Program
    {
        private const string LoadingCompletedTemplate = @"
Count of events: {0}, count of buckets: {1} 
Event range: {2}-{3}μs ({4}ms)";
        private const string ChoiceTemplate = "What do you want to do? Enter 'p' to show payloads, enter 'c' to show charts. \nPlease make your choice:  ";

        private const string InputRangeTemplate = "Input a range of time (μs) you want to select events from in the following format: [start]-[end]. Press Enter to exit.";
        private static Regex _rangeRgx = new Regex(@"^(\d+)\s*-\s*(\d+)\s*$");


        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Usage: dotnet EventUtility.dll {pathToGeneratedFile}");
                Console.WriteLine("Example: dotnet EventUtility.dll ..\\..\\..\\..\\data\\events.bin");
                return;
            }

            Console.ForegroundColor = ConsoleColor.White;
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            try
            {
                Console.Write(ChoiceTemplate);
                bool showPayloads = string.Equals(Console.ReadKey().KeyChar.ToString(), "p", StringComparison.OrdinalIgnoreCase);
                Console.WriteLine();

                LoadEvents(args[0], showPayloads);
            }
            catch (Exception exc)
            {
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(exc.ToString());
            }
            finally
            {
                Console.ForegroundColor = ConsoleColor.Gray;
            }

        }

        private static void LoadEvents(string filePath, bool showPayloads)
        {
            var eventsApi = new ApiFacade
            {
                ProgressHandler = new ConsoleProgress()
            };

           
            var container = eventsApi.LoadEventsFromFileAsync(filePath, showPayloads ?
                LoadStrategyType.LoadEventsAndPayloads : LoadStrategyType.LoadEventsForChart)
                .GetAwaiter()
                .GetResult();


            var eventsTotalCount = container.Buckets.Sum(b => (long)b.Events.Length);
            Bucket lastBucket = container.Buckets.Length > 0 ? container.Buckets[container.Buckets.Length - 1] : null;
            long startTimestamp = container.FirstTimestamp,
                 endTimestamp = lastBucket?.GetAbsoluteTimeForEvent(lastBucket.GetLastEvent()) ?? 0;


            Console.WriteLine(
                LoadingCompletedTemplate,
                eventsTotalCount,
                container.Buckets.Length,
                startTimestamp,
                endTimestamp,
                (endTimestamp - startTimestamp) / 1000
            );


            while (true)
            {
                Console.WriteLine(InputRangeTemplate);
                var matches = _rangeRgx.Match(Console.ReadLine());
                if (!matches.Success)
                    break;

                long start = long.Parse(matches.Groups[1].Value);
                long end = long.Parse(matches.Groups[2].Value);

                Console.WriteLine("Your range length is {0:0.00}s.\n", (end - start) / (double)1000000);
                if (showPayloads)
                {
                    DisplayValidPayloads(container, start, end);
                }
                else
                {
                    ShowCharts(container, start, end);
                }
                Console.WriteLine("\n[END]\n\n");
            }
        }

        private static void ShowCharts(BucketContainer container, long start, long end)
        {
            RunPerfomanceTests(container, start, end);

            Console.WriteLine("Here are densities for chart of width equal to 60px");
            ShowDensities(container, start, end, (end - start) / 60);


            Console.WriteLine("Here are densities for chart of width equal to 120px");
            ShowDensities(container, start, end, (end - start) / 120);
        }

        private static void RunPerfomanceTests(BucketContainer container, long start, long end)
        {
            bool isWarmUp = true;
            int tries = 0;

            while (tries < 2)
            {
                var stopWatch = new Stopwatch();
                foreach (var segmentSize in container.GetPreferredSegmentSizes(start, end, 60))
                {
                    long tStart = start, tEnd = Math.Min(start + segmentSize * 2000, end);
                    stopWatch.Start();
                    var densities = container.GetDensities(tStart, tEnd, segmentSize);
                    var elapsed = stopWatch.ElapsedMilliseconds;
                    stopWatch.Reset();

                    if (!isWarmUp)
                        Console.WriteLine("Segment size {0,10}: {1,4}ms elapsed for the range with length {2}μs: average density = {3}",
                           segmentSize, elapsed, tEnd-tStart, densities.Average());
                }
                tries++;
                isWarmUp = false;
            }
        }

        private static void ShowDensities(BucketContainer container, long start, long end, long segmentSize)
        {
            var payloads = container.GetEvents(start, end);

            var densities = container.GetDensities(start, end, segmentSize);
            
            //Console.ForegroundColor = ConsoleColor.Black;
            foreach (var density in densities)
            {
                Console.BackgroundColor = ConsoleColor.Black;
                Console.Write($"{{0,{7}:0.00}}% ", density * 100);

                Console.BackgroundColor = ConsoleColor.Gray;
                var percent = (int)(density * (Console.WindowWidth - 10));
                for (var i = 1; i <= percent; i++)
                {
                    Console.Write(' ');
                }
                Console.WriteLine();
                
            }
            Console.WriteLine();
            //Console.ForegroundColor = ConsoleColor.White;
            Console.BackgroundColor = ConsoleColor.Black;
        }

        private static void DisplayValidPayloads(BucketContainer container, long start, long end)
        {
            var payloads = container.GetPayloads(start, end);
            Console.WriteLine("There are {0} events in your range (display limit is 100 events):", payloads.Count);

            int count = 0;
            foreach (ref readonly Payload payload in payloads)
            {
                Console.WriteLine("{0}. Payload: {2}, {3}, {4}, {5}",
                    count + 1,
                    0,
                    //baseTimestamp + (ev.BucketOffset * Bucket.MaxRelativeEventTime) + ev.Event.RelativeTime,
                    payload.First,
                    payload.Second,
                    payload.Third,
                    payload.Fourth);

                if (++count >= 100)
                    break;
            }
        }
    }
}
