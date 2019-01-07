using EventsDomain;

namespace EventsProcessingAPI.DataRead
{
    internal interface IBucketBuilder
    {
        void AddEvent(in RealEvent ev);
        void AddEventWithPayload(in RealEvent ev, in Payload payload);
        Bucket Build(bool withPayloads);
        bool IsFitIntoCurrentBucket(long eventTime);
        void StartNewBucket(long firstEventTime);
        bool HasEvents { get; }
    }
}