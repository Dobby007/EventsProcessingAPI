using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace EventsDomain
{
    /// <summary>
    /// Memory-friendly version of the event. Its' size is 4 bytes.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct Event
    {
        private const byte LowEventTimeMask = 0b1111;
        public const byte CpuTickMask = 0b1111;

        /// <summary>
        /// High 16 bits of the event time in microseconds
        /// </summary>
        public readonly ushort EventTimeHigh;

        /// <summary>
        /// Low 4 bits of the event time in microseconds and CPU ticks count. This field is composed as concatenation of 2 numbers: 0bLLLLCCCC,
        /// where LLLL is low  4 bits of the event time in microseconds and CCCC is a number of CPU ticks in the range of 0-9.
        /// </summary>
        public readonly byte EventTimeLow;

        public readonly EventType EventType;

        public long RelativeTime
        {
            get
            {
                return (EventTimeHigh << 4 | EventTimeLow >> 4) * 10 +
                    (EventTimeLow & CpuTickMask);
            }
        }

        /// <summary>
        /// Constructor of the event
        /// </summary>
        /// <param name="eventType">Type of the event: start or stop</param>
        /// <param name="bucketEventTime">Time of the event relative to its bucket. Allowed range: 0 - 9 999 999 (1 second).</param>
        public Event(EventType eventType, uint bucketEventTime)
        {
            EventType = eventType;

            uint timeInMicroseconds = bucketEventTime / 10;
            EventTimeHigh = unchecked((ushort)(timeInMicroseconds >> 4));
            EventTimeLow = (byte)((timeInMicroseconds & 0b1111) << 4 | (bucketEventTime % 10));
        }
    }
}
