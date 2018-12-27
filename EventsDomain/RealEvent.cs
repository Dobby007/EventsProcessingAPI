using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace EventsDomain
{
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
