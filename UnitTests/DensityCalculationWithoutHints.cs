using EventsProcessingAPI.Density;
using System;
using UnitTests.Fixtures;
using Xunit;

namespace UnitTests
{
    public class DensityCalculationWithoutHints : IClassFixture<SampleEventsFixture>
    {
        SampleEventsFixture _fixture;

        public DensityCalculationWithoutHints(SampleEventsFixture fixture)
        {
            _fixture = fixture;
        }


        [Fact]
        public void AllDensitiesEqualOne()
        {
            var densities = DensityCalculator.GetDensities(_fixture.SampleEvents1, 31 * Durations.Second, 35 * Durations.Second - 1, Durations.Second);
            Assert.NotEmpty(densities);
            Assert.All(densities, d => Assert.Equal(1, d));
        }

        [Fact]
        public void NoDensities()
        {
            var densities = DensityCalculator.GetDensities(_fixture.SampleEvents1, 255 * Durations.Second, 260 * Durations.Second - 1, Durations.Second);
            Assert.Empty(densities);
        }
    }
}
