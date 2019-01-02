using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace EventsDomain
{
    /// <summary>
    /// Structure that represents a real event. Its' size is 9 bytes.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public readonly struct RealEvent
    {
        public readonly EventType EventType;
        public readonly long Ticks;

        public RealEvent(EventType eventType, long ticks)
        {
            EventType = eventType;
            Ticks = ticks;
        }
    }
}
