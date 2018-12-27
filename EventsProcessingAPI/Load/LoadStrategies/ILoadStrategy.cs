using System;
using System.Threading.Tasks;

namespace EventsProcessingAPI.Load.LoadStrategies
{
    public interface ILoadStrategy : IDisposable
    {
        IProgress<int> ProgressHandler { get; set; }
        Task<BucketContainer> LoadEventsAsync();
    }
}