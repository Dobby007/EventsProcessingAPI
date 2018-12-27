using EventsDomain;
using System.Collections.Generic;

namespace EventsProcessingAPI.Ranges
{
    class EventComparer : IComparer<Event>
    {
        private static IComparer<ushort> DefaultComparer = Comparer<ushort>.Default;
        public int Compare(Event x, Event y)
        {
            return DefaultComparer.Compare(x.RelativeTime, y.RelativeTime);
        }
    }
}
