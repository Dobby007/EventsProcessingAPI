using EventsProcessingAPI;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace DifferentStrategiesTest
{
    class Program
    {
        private static Regex _rangeRgx = new Regex(@"^(\d+)\s*-\s*(\d+)\s*$");
        private const string InputRangeTemplate = "Input a range of time (μs) you want to select events from in the following format: [start]-[end]. Press Enter to exit.";

        static void Main(string[] args)
        {
            while (true)
            {
                Console.WriteLine(InputRangeTemplate);
                var matches = _rangeRgx.Match(Console.ReadLine());
                if (!matches.Success)
                    break;

                long start = long.Parse(matches.Groups[1].Value) * 10;
                long end = long.Parse(matches.Groups[2].Value) * 10 + 9;

                ExportAverageDensities(args[0], start, end, "output1.txt", LoadStrategyType.LoadOnlyEvents);
                ExportAverageDensities(args[0], start, end, "output2.txt", LoadStrategyType.LoadEventsForChart);
                
            }

        }

        static void ExportAverageDensities(string filePath, long start, long end, string outputFile, LoadStrategyType loadStrategyType)
        {
            var apiFacade = new ApiFacade();
            apiFacade.ProgressHandler = new ConsoleProgress();

            var container = apiFacade.LoadEventsFromFileAsync(filePath, loadStrategyType)
                    .GetAwaiter()
                    .GetResult();

            Console.WriteLine();
            Console.WriteLine("Please enter minimum segment size: ");
            var minSegmentSize = long.Parse(Console.ReadLine());

            using (var sr = new StreamWriter(new FileStream(outputFile, FileMode.Create)))
            {
                foreach (var segmentSize in container.GetPreferredSegmentSizes(start, end, 60).SkipWhile(s => s < minSegmentSize))
                {
                    long tStart = start, tEnd = Math.Min(start + segmentSize * 2000, end);
                    var densities = container.GetDensities(tStart, tEnd, segmentSize);

                    sr.WriteLine("\nSegmentSize: " + segmentSize);
                    foreach (var density in densities)
                        sr.WriteLine(density);
                }
            }
        }
    }
}
