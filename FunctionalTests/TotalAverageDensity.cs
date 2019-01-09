using EventsDomain;
using EventsProcessingAPI;
using FunctionalTests.Fixtures;
using System;
using System.IO;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace FunctionalTests
{
    [Collection(CollectionFixtures.RandomEventFilesCollection)]
    public class TotalAverageDensity
    {
        private readonly FileWith100kEventsFixture File100k;
        private readonly FileWith500kEventsFixture File500k;
        private readonly FileWith1mEventsFixture File1m;
        private readonly FileWith10mEventsFixture File10m;
        private readonly ITestOutputHelper _output;

        public TotalAverageDensity(FileWith100kEventsFixture file100k, FileWith500kEventsFixture file500k,
            FileWith1mEventsFixture file1m, FileWith10mEventsFixture file10m, ITestOutputHelper output)
        {
            File100k = file100k;
            File500k = file500k;
            File1m = file1m;
            File10m = file10m;
            _output = output;
        }

        [Fact]
        public void TestFileWith100kEvents()
        {
            RunTest(File100k.FileName);
        }

        [Fact]
        public void TestFileWith500kEvents()
        {
            RunTest(File500k.FileName);
        }

        [Fact]
        public void TestFileWith1mEvents()
        {
            RunTest(File1m.FileName);
        }

        [Fact]
        public void TestFileWith10mEvents()
        {
            RunTest(File10m.FileName);
        }

        private void RunTest(string fileName)
        {
            var density1 = CalculateAverageDensityWithApi(fileName, out long start, out long end);
            var density2 = CalculateAverageDensityBySimpleMethod(fileName, start, end);

            _output.WriteLine(
                "Average density calculated with API = {0}, average density calculated by simple method = {1}", 
                density1, 
                density2);

            Assert.Equal(density2, density1, 5);
        }

        static double CalculateAverageDensityBySimpleMethod(string filePath, long start, long end)
        {
            using (var reader = new EventReader(new FileStream(filePath, FileMode.Open, FileAccess.Read)))
            {
                reader.StartReadingEvents(false, null);

                long lastEventTime = default;
                long currentEventTime = default;
                EventType lastEventType = default;

                int count = 0;
                int startEventsCount = 0;
                int endEventsCount = 0;
                double filled = 0;
                double unfilled = 0;


                foreach (var ev in reader.GetAllEvents())
                {
                    currentEventTime = ev.Ticks;

                    if (currentEventTime < start)
                        continue;
                    
                    Assert.True(count == 0 || ev.EventType != lastEventType);

                    if (count > 0)
                    {
                        var diff = currentEventTime > end ? end : currentEventTime - lastEventTime;
                        switch (lastEventType)
                        {
                            case EventType.Start:
                                filled += diff;
                                endEventsCount++;
                                break;
                            case EventType.Stop:
                                unfilled += diff;
                                startEventsCount++;
                                break;
                        }
                    }
                    else if (ev.EventType == EventType.Start)
                    {
                        startEventsCount++;
                    }
                    

                    if (ev.EventType == EventType.Stop)
                        Assert.Equal(endEventsCount, startEventsCount);

                    if (currentEventTime > end)
                        break;

                    lastEventTime = currentEventTime;
                    lastEventType = ev.EventType;
                    count++;

                    
                }


                double averageDensity = filled / (double)(filled + unfilled);
                return averageDensity;
            }
        }

        static double CalculateAverageDensityWithApi(string filePath, out long start, out long end)
        {
            var apiFacade = new ApiFacade();
            //apiFacade.ProgressHandler = new ConsoleProgress();

            var container = apiFacade.LoadEventsFromFileAsync(filePath, LoadStrategyType.LoadEventsForChart)
                    .GetAwaiter()
                    .GetResult();

            start = container.FirstTimestamp;
            end = container.LastTimestamp;

            long[] segmentSizes = container.GetPreferredSegmentSizes(1000);
            long segmentSize = segmentSizes.Last();

            var densitiesBySegments = container.GetDensities(start, end, segmentSize);
            var averageDensity = densitiesBySegments.Average();

            return averageDensity;
        }
    }
}
