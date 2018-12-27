using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EventsDomain;

namespace EventsProcessingAPI
{
    public interface IEventReader : IDisposable
    {
        uint TotalEventsCount { get; }
        long FirstTimestamp { get; }
        IEnumerable <Bucket> GetAllBuckets();
        Task StartReadingEvents(bool enablePayload, IProgress<int> progress);
    }
}