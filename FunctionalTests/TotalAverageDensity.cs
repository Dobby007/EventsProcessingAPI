using System;
using Xunit;

namespace FunctionalTests
{
    public class TotalAverageDensity
    {
        [Fact]
        public void Test1()
        {

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
