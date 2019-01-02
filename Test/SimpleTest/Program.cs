using EventsDomain;
using EventsProcessingAPI;
using EventsProcessingAPI.Density;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace SimpleTest
{
    class Program
    {
        private static Regex _rangeRgx = new Regex(@"^(\d+)\s*-\s*(\d+)\s*$");
        private const string InputRangeTemplate = "Input a range of time (μs) you want to select events from in the following format: [start]-[end]. Press Enter to exit.";
        private const string AverageDensityTemplate = "Average density on the whole range: {0:0.00}%";

        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Usage: dotnet SimpleTest.dll {pathToGeneratedFile}");
                Console.WriteLine("Example: dotnet SimpleTest.dll ..\\..\\..\\..\\..\\data\\events.bin");
                return;
            }

            Console.OutputEncoding = System.Text.Encoding.UTF8;

            while (true)
            {
                Console.WriteLine(InputRangeTemplate);
                var matches = _rangeRgx.Match(Console.ReadLine());
                if (!matches.Success)
                    break;

                long start = long.Parse(matches.Groups[1].Value) * 10;
                long end = long.Parse(matches.Groups[2].Value) * 10 + 9;
                
                var density1 = CalculateAverageDensityBySimpleMethod(args[0], start, end);
                var density2 = CalculateAverageDensityWithApi(args[0], start, end);
                
                Console.WriteLine("Densities are equal: {0}", Math.Abs(density1 - density2) < 0.01);
            }

        }

        static double CalculateAverageDensityBySimpleMethod(string filePath, long start, long end)
        {
            using (var reader = new EventReader(new FileStream(filePath, FileMode.Open, FileAccess.Read)))
            {
                reader.StartReadingEvents(false, new ConsoleProgress());

                long lastEventTime = default;
                long currentEventTime = default;
                EventType lastEventType = default;

                int count = 0;
                long filled = 0;
                long unfilled = 0;
                

                foreach (var ev in reader.GetAllEvents())
                {
                    currentEventTime = ev.Ticks;

                    if (currentEventTime < start)
                        continue;

                    if (count > 0)
                    {
                        var diff = currentEventTime > end ? end : currentEventTime - lastEventTime;
                        switch (lastEventType)
                        {
                            case EventType.Start:
                                filled += diff;
                                break;
                            case EventType.Stop:
                                unfilled += diff;
                                break;
                        }
                    }

                    if (currentEventTime > end)
                        break;

                    lastEventTime = currentEventTime;
                    lastEventType = ev.EventType;
                    count++;
                }


                double averageDensity = filled / (double)(filled + unfilled);
                Console.WriteLine();
                Console.WriteLine(AverageDensityTemplate, averageDensity * 100);

                return averageDensity;
            }
        }
        
        static double CalculateAverageDensityWithApi(string filePath, long start, long end)
        {
            var apiFacade = new ApiFacade();
            apiFacade.ProgressHandler = new ConsoleProgress();

            var container = apiFacade.LoadEventsFromFileAsync(filePath, LoadStrategyType.LoadEventsForChart)
                    .GetAwaiter()
                    .GetResult();

            Console.WriteLine();
            Console.WriteLine("Please enter segment size: ");
            var segmentSize = long.Parse(Console.ReadLine());
            var densitiesBySegments = container.GetDensities(start, end, segmentSize);
            var averageDensity = densitiesBySegments.Average();
            Console.WriteLine(AverageDensityTemplate, averageDensity * 100);

            return averageDensity;
        }

        
    }
}
