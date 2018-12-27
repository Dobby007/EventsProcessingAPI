using EventsDomain;
using System.Collections.Generic;

namespace EventsProcessingAPI.Ranges
{
    class BucketOffsetComparer : IComparer<Bucket>
    {
        private static IComparer<long> DefaultComparer = Comparer<long>.Default;
        public int Compare(Bucket x, Bucket y)
        {
            return DefaultComparer.Compare(x.Offset, y.Offset);
        }
    }
}
