using System;
using System.Collections.Generic;
using System.Text;

namespace EventsDomain
{
    public readonly struct Event
    {
        public readonly EventType EventType;

        public readonly ushort RelativeTime;

        public Event(EventType eventType, ushort relativeTime)
        {
            EventType = eventType;
            RelativeTime = relativeTime;
        }
    }
}
