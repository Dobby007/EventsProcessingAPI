using System;
using System.Collections.Generic;
using System.Text;

namespace EventsProcessingAPI.Common
{
    public readonly struct SegmentSize : IEquatable<SegmentSize>
    {
        public readonly long RequestedValue;
        public readonly long DisplayedValue;

        public SegmentSize(long requestedValue, long displayedValue)
        {
            RequestedValue = requestedValue;
            DisplayedValue = displayedValue;
        }

        public SegmentSize(long value)
        {
            RequestedValue = value;
            DisplayedValue = value;
        }

        public double ScaleCoefficient => RequestedValue / (double)DisplayedValue;

        public bool NeedToScale => ScaleCoefficient != 1;

        public bool Equals(SegmentSize other)
        {
            return RequestedValue == other.RequestedValue && DisplayedValue == other.DisplayedValue;
        }

        public override bool Equals(object obj)
        {
            return Equals((SegmentSize)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 13;
                hash = (hash * 7) + RequestedValue.GetHashCode();
                hash = (hash * 7) + DisplayedValue.GetHashCode();

                return hash;
            }
        }
    }
}
