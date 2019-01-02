using EventsDomain;
using System.Collections.Generic;

namespace EventsProcessingAPI.Ranges
{
    class EventComparer : IComparer<Event>
    {
        public int Compare(Event x, Event y)
        {
            if (x.EventTimeHigh > y.EventTimeHigh)
                return 1;
            else if (x.EventTimeHigh < y.EventTimeHigh)
                return -1;
            else
            {
                if (x.EventTimeLow > y.EventTimeLow)
                    return 1;
                else if (x.EventTimeLow < y.EventTimeLow)
                    return -1;
                else
                    return 0;
            }
        }
    }
}
