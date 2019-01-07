using EventsProcessingAPI.Common.Pipeline;
using EventsProcessingAPI.DataRead;
using EventsProcessingAPI.Density;
using System;
using System.Threading.Tasks;

namespace EventsProcessingAPI.Load.LoadStrategies
{
    public class LoadEventsForChartStrategy : ILoadStrategy
    {
        private readonly IEventReader _eventReader;
        private readonly bool _loadPayloads;
        public IProgress<int> ProgressHandler { get; set; }


        public LoadEventsForChartStrategy(IEventReader eventReader, bool loadPayloads = true)
        {
            _eventReader = eventReader ?? throw new ArgumentNullException(nameof(eventReader));
            _loadPayloads = loadPayloads;
        }

        
        public async Task<BucketContainer> LoadEventsAsync()
        {
            var pipeline = new ProcessPipeline() { new DensityPrecalculation() };
            var processor = new ParallelEventsProcessor(pipeline, ProgressHandler);
            return await processor.ProcessEventsAsync(_eventReader, _loadPayloads);
        }

        #region IDisposable Support
        private bool disposedValue = false;


        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _eventReader?.Dispose();
                }

                disposedValue = true;
            }
        }
        
        public void Dispose()
        {
            Dispose(true);
        }
        #endregion

    }
}
