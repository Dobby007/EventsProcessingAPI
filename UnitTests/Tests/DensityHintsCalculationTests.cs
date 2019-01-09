using EventsProcessingAPI;
using EventsProcessingAPI.Density;
using System;
using System.Linq;
using UnitTests.Common;
using UnitTests.Fixtures;
using Xunit;

namespace UnitTests.Tests
{
    public class DensityHintsCalculationTests : IClassFixture<SampleEventsFixture>
    {
        SampleEventsFixture _fixture;
        DensityPrecalculation _densityPrecalculationProcess;


        public DensityHintsCalculationTests(SampleEventsFixture fixture)
        {
            _fixture = fixture;
            _densityPrecalculationProcess = new DensityPrecalculation();
        }
        
        [Fact]
        public void DensityHintsAreEqualToDensities()
        {
            BucketContainer bucketContainer = _fixture.SampleEvents3;
            var densitiesBySeconds = bucketContainer.GetDensities(bucketContainer.FirstTimestamp, bucketContainer.LastTimestamp, Durations.Second);
            var densitiesByMinutes = bucketContainer.GetDensities(bucketContainer.FirstTimestamp, bucketContainer.LastTimestamp, Durations.Minute);

            _densityPrecalculationProcess.Complete(bucketContainer);

            var hints = bucketContainer.DensityHintContainer;

            Assert.NotNull(hints);

            Assert.Equal(densitiesBySeconds.Length, (int)hints.Seconds.Keys.Max());
            Assert.Equal(densitiesByMinutes.Length, (int)hints.Minutes.Keys.Max());

            foreach (var hint in hints.Seconds)
            {
                Assert.True(hint.Key >= 1);
                Assert.Equal(densitiesBySeconds[hint.Key - 1], hint.Value);
            }

            foreach (var hint in hints.Minutes)
            {
                Assert.True(hint.Key >= 1);
                Assert.Equal(densitiesByMinutes[hint.Key - 1], hint.Value);
            }
        }

        [Fact]
        public void DensityHintsCountIsCorrect()
        {
            BucketContainer bucketContainer = _fixture.SampleEvents3;
            _densityPrecalculationProcess.Complete(bucketContainer);

            var hints = bucketContainer.DensityHintContainer;

            Assert.NotNull(hints);

            var totalDuration = bucketContainer.LastTimestamp - bucketContainer.FirstTimestamp;
            Assert.Equal(Math.Ceiling(totalDuration / (double)Durations.Second), (int)hints.Seconds.Keys.Max());
            Assert.Equal(Math.Ceiling(totalDuration / (double)Durations.Minute), (int)hints.Minutes.Keys.Max());

        }
    }
}
