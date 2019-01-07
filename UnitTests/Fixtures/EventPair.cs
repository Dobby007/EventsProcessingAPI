using EventsProcessingAPI.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace UnitTests.Fixtures
{
    readonly struct EventPair
    {
        public readonly bool IsAbsoluteOffset;
        public readonly long Offset;
        public readonly long Duration;
        public readonly TimeUnit DurationTimeUnit;

        public EventPair(long offset, long duration, TimeUnit durationTimeUnit, bool isAbsoluteOffset = false)
        {
            Offset = offset;
            Duration = duration;
            DurationTimeUnit = durationTimeUnit;
            IsAbsoluteOffset = isAbsoluteOffset;
        }
    }
}
