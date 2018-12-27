using System.Threading.Tasks;

namespace EventsProcessingAPI.Load
{
    interface IEventsProcessor
    {
        Task<BucketContainer> ProcessEventsAsync(IEventReader reader, bool enablePayloads);
    }
}