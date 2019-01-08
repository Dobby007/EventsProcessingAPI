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
        private readonly FileWith1mEventsFixture File1m;
        private readonly FileWith10mEventsFixture File10m;


        public DensityCalculationUsingDifferentStrategies(FileWith100kEventsFixture file100k, FileWith500kEventsFixture file500k,
            FileWith1mEventsFixture file1m, FileWith10mEventsFixture file10m)
        {
            File100k = file100k;
            File500k = file500k;
            File1m = file1m;
            File10m = file10m;
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
            var apiFacade = new ApiFacade();
            //apiFacade.ProgressHandler = new ConsoleProgress();

            var containerWithoutDensityHints = apiFacade.LoadEventsFromFileAsync(fileName, LoadStrategyType.LoadOnlyEvents)
                    .GetAwaiter()
                    .GetResult();

            var containerWithDensityHints = apiFacade.LoadEventsFromFileAsync(fileName, LoadStrategyType.LoadEventsForChart)
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
                double[] densitiesWithHints = containerWithDensityHints.GetDensities(tStart, tEnd, segmentSize);

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
