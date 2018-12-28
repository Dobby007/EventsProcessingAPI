using EventsDomain;
using EventsProcessingAPI;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace EventUtility
{
    class InteractiveUserMode
    {
        private const string InputRangeTemplate = "Input a range of time (μs) you want to select events from in the following format: [start]-[end]. Press Enter to exit.";
        private static Regex _rangeRgx = new Regex(@"^(\d+)\s*-\s*(\d+)\s*$");

        public InteractiveModeType InteractiveModeType { get; }
        public BucketContainer BucketContainer { get; }

        public InteractiveUserMode(InteractiveModeType interactiveModeType, BucketContainer bucketContainer)
        {
            InteractiveModeType = interactiveModeType;
            BucketContainer = bucketContainer;
        }


        public void Start()
        {

            while (true)
            {
                Console.WriteLine(InputRangeTemplate);
                var matches = _rangeRgx.Match(Console.ReadLine());
                if (!matches.Success)
                    break;

                long start = long.Parse(matches.Groups[1].Value);
                long end = long.Parse(matches.Groups[2].Value);

                Console.WriteLine("Your range length is {0:0.00}s.\n", (end - start) / (double)1000000);
                switch (InteractiveModeType)
                {
                    case InteractiveModeType.ShowPayloads:
                        DisplayValidPayloads(BucketContainer, start, end);
                        break;
                    case InteractiveModeType.ShowCharts:
                        ShowCharts(BucketContainer, start, end);
                        break;
                }
                Console.WriteLine("\n[END]\n\n");
            }
        }

        private static void ShowCharts(BucketContainer container, long start, long end)
        {
            ShowChart(container, start, end, 60);
            ShowChart(container, start, end, 120);
        }

        private static void ShowChart(BucketContainer container, long start, long end, ushort chartWidth)
        {
            long[] preferredSizes = container.GetPreferredSegmentSizes(start, end, chartWidth);

            if (preferredSizes.Length < 1)
                throw new InvalidOperationException();

            long segmentSize = preferredSizes[preferredSizes.Length - 1];
            end = start + segmentSize * chartWidth;
            Console.WriteLine("Here are densities for chart of width equal to {0}px", chartWidth);
            
            var densities = container.GetDensities(start, end, segmentSize);
            
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
