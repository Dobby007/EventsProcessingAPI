using EventsProcessingAPI.Common;
using System;
using System.Collections.Generic;
using System.Text;
using UnitTests.Common;
using Xunit;

namespace UnitTests.Tests
{
    public class TimeUnitHelperTests
    {
        [Fact]
        public void FloorTimeUnitIsCorrect()
        {
            Assert.Equal(TimeUnit.CpuTick, TimeUnitHelpers.GetFloorTimeUnit(Durations.CpuTick));
            Assert.Equal(TimeUnit.CpuTick, TimeUnitHelpers.GetFloorTimeUnit(Durations.CpuTick * 9));

            Assert.Equal(TimeUnit.Microsecond, TimeUnitHelpers.GetFloorTimeUnit(Durations.Microsecond));

            Assert.Equal(TimeUnit.Second, TimeUnitHelpers.GetFloorTimeUnit(Durations.Second + 1));
            Assert.Equal(TimeUnit.Second, TimeUnitHelpers.GetFloorTimeUnit(Durations.Second));

            Assert.Equal(TimeUnit.Minute, TimeUnitHelpers.GetFloorTimeUnit(Durations.Minute + 1));
            Assert.Equal(TimeUnit.Minute, TimeUnitHelpers.GetFloorTimeUnit(Durations.Minute));

            Assert.Equal(TimeUnit.Hour, TimeUnitHelpers.GetFloorTimeUnit(Durations.Hour + 1));
            Assert.Equal(TimeUnit.Hour, TimeUnitHelpers.GetFloorTimeUnit(Durations.Hour));
            Assert.Equal(TimeUnit.Hour, TimeUnitHelpers.GetFloorTimeUnit(Durations.Hour * 1000));

        }

        [Fact]
        public void CeilingTimeUnitIsCorrect()
        {
            Assert.Equal(TimeUnit.CpuTick, TimeUnitHelpers.GetCeilingTimeUnit(Durations.CpuTick));

            Assert.Equal(TimeUnit.Microsecond, TimeUnitHelpers.GetCeilingTimeUnit(Durations.CpuTick * 9));
            Assert.Equal(TimeUnit.Microsecond, TimeUnitHelpers.GetCeilingTimeUnit(Durations.Microsecond));
            Assert.Equal(TimeUnit.Microsecond, TimeUnitHelpers.GetCeilingTimeUnit(Durations.Microsecond - 1));

            Assert.Equal(TimeUnit.Second, TimeUnitHelpers.GetCeilingTimeUnit(Durations.Millisecond * 999));
            Assert.Equal(TimeUnit.Second, TimeUnitHelpers.GetCeilingTimeUnit(Durations.Millisecond * 999 + 9));
            Assert.Equal(TimeUnit.Second, TimeUnitHelpers.GetCeilingTimeUnit(Durations.Millisecond * 999 + 10));
            Assert.Equal(TimeUnit.Second, TimeUnitHelpers.GetCeilingTimeUnit(Durations.Second));

            Assert.Equal(TimeUnit.Minute, TimeUnitHelpers.GetCeilingTimeUnit(Durations.Second + 1));
            Assert.Equal(TimeUnit.Minute, TimeUnitHelpers.GetCeilingTimeUnit(Durations.Second * 59 + Durations.Millisecond * 999));
            Assert.Equal(TimeUnit.Minute, TimeUnitHelpers.GetCeilingTimeUnit(Durations.Minute));

            Assert.Equal(TimeUnit.Hour, TimeUnitHelpers.GetCeilingTimeUnit(Durations.Minute + 1));
            Assert.Equal(TimeUnit.Hour, TimeUnitHelpers.GetCeilingTimeUnit(Durations.Minute * 59));
            Assert.Equal(TimeUnit.Hour, TimeUnitHelpers.GetCeilingTimeUnit(Durations.Minute * 59 + Durations.Second * 59));

            Assert.Equal(TimeUnit.Hour, TimeUnitHelpers.GetCeilingTimeUnit(Durations.Hour + 1));
            Assert.Equal(TimeUnit.Hour, TimeUnitHelpers.GetCeilingTimeUnit(Durations.Hour));
            Assert.Equal(TimeUnit.Hour, TimeUnitHelpers.GetCeilingTimeUnit(Durations.Hour * 1000));
        }

        [Fact]
        public void NextTimeUnitIsCorrect()
        {
            bool result;
            TimeUnit next;

            result = TimeUnitHelpers.TryGetNextTimeUnit(TimeUnit.CpuTick, out next);
            Assert.True(result);
            Assert.Equal(TimeUnit.Microsecond, next);

            result = TimeUnitHelpers.TryGetNextTimeUnit(TimeUnit.Microsecond, out next);
            Assert.True(result);
            Assert.Equal(TimeUnit.Millisecond, next);

            result = TimeUnitHelpers.TryGetNextTimeUnit(TimeUnit.Millisecond, out next);
            Assert.True(result);
            Assert.Equal(TimeUnit.Second, next);

            result = TimeUnitHelpers.TryGetNextTimeUnit(TimeUnit.Second, out next);
            Assert.True(result);
            Assert.Equal(TimeUnit.Minute, next);

            result = TimeUnitHelpers.TryGetNextTimeUnit(TimeUnit.Minute, out next);
            Assert.True(result);
            Assert.Equal(TimeUnit.Hour, next);

            result = TimeUnitHelpers.TryGetNextTimeUnit(TimeUnit.Hour, out next);
            Assert.False(result);
        }


    }
}
