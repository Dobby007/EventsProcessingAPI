using EventsProcessingAPI.DataRead;
using System.Threading.Tasks;

namespace EventsProcessingAPI.Load
{
    public interface IEventsProcessor
    {
        Task<BucketContainer> ProcessEventsAsync(IEventReader reader, bool enablePayloads);
    }
}