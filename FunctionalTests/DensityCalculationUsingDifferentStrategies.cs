using EventsProcessingAPI;
using FunctionalTests.Fixtures;
using System;
using System.Collections.Generic;
using Xunit;
using Xunit.Sdk;

namespace FunctionalTests
{
    [Collection(CollectionFixtures.RandomEventFilesCollection)]
    public class DensityCalculationUsingDifferentStrategies
    {
        private readonly FileWith100kEventsFixture File100k;
        private readonly FileWith500kEventsFixture File500k;

        public DensityCalculationUsingDifferentStrategies(FileWith100kEventsFixture file100k, FileWith500kEventsFixture file500k)
        {
            File100k = file100k;
            File500k = file500k;
        }

        [Fact]
        public void TestFileWith100kEvents()
        {
            var apiFacade = new ApiFacade();
            //apiFacade.ProgressHandler = new ConsoleProgress();

            var containerWithoutDensityHints = apiFacade.LoadEventsFromFileAsync(File100k.FileName, LoadStrategyType.LoadOnlyEvents)
                    .GetAwaiter()
                    .GetResult();

            var containerWithDensityHints = apiFacade.LoadEventsFromFileAsync(File100k.FileName, LoadStrategyType.LoadEventsForChart)
                    .GetAwaiter()
                    .GetResult();

            Assert.Equal(containerWithoutDensityHints.FirstTimestamp, containerWithDensityHints.FirstTimestamp);
            Assert.Equal(containerWithoutDensityHints.LastTimestamp, containerWithDensityHints.LastTimestamp);
            Assert.Equal(containerWithoutDensityHints.Buckets.Length, containerWithDensityHints.Buckets.Length);

            long firstTimestamp = containerWithoutDensityHints.FirstTimestamp,
                 lastTimestamp = containerWithoutDensityHints.LastTimestamp;

            var assertionFailedList = new List<AssertionInfo>();

            foreach (var segmentSize in containerWithoutDensityHints.GetPreferredSegmentSizes(firstTimestamp, lastTimestamp, 60))
            {
                long tStart = firstTimestamp, tEnd = Math.Min(firstTimestamp + segmentSize * 10000, lastTimestamp);

                double[] densitiesWithoutHints = containerWithoutDensityHints.GetDensities(tStart, tEnd, segmentSize);
                double[] densitiesWithHints = containerWithoutDensityHints.GetDensities(tStart, tEnd, segmentSize);

                Assert.Equal(densitiesWithoutHints.Length, densitiesWithHints.Length);

                for (var i = 0; i < densitiesWithoutHints.Length; i++)
                {
                    try
                    {
                        Assert.Equal(densitiesWithoutHints[i], densitiesWithHints[i], 10);
                    }
                    catch (EqualException)
                    {
                        assertionFailedList.Add(new AssertionInfo
                        {
                            Expected = densitiesWithoutHints[i],
                            Actual = densitiesWithHints[i],
                            Index = i,
                            SegmentSize = segmentSize
                        });
                        
                    }
                }
            }

            if (assertionFailedList.Count > 0)
            {
                throw new DensitiesEqualityException(assertionFailedList);
            }
        }
    }
}
