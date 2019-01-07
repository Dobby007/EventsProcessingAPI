using EventsProcessingAPI.DataRead;
using EventsProcessingAPI.Load.LoadStrategies;
using System;
using System.IO;
using System.Threading.Tasks;

namespace EventsProcessingAPI
{
    public class ApiFacade
    {
        public IProgress<int> ProgressHandler { get; set; }

        public async Task<BucketContainer> LoadEventsFromFileAsync(string filePath, LoadStrategyType loadStrategyType)
        {
            var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            using (var loadStrategy = GetLoadStrategy(loadStrategyType, new EventReader(fs, new BucketBuilder())))
            {
                loadStrategy.ProgressHandler = ProgressHandler;
                return await loadStrategy.LoadEventsAsync();
            }
        }

        public async Task<BucketContainer> LoadEventsFromStreamAsync(Stream stream, LoadStrategyType loadStrategyType)
        {
            using (var loadStrategy = GetLoadStrategy(loadStrategyType, new EventReader(stream, new BucketBuilder())))
            {
                loadStrategy.ProgressHandler = ProgressHandler;
                return await loadStrategy.LoadEventsAsync();
            }
        }

        private ILoadStrategy GetLoadStrategy(LoadStrategyType loadStrategyType, IEventReader reader)
        {
            switch (loadStrategyType)
            {
                case LoadStrategyType.LoadEventsAndPayloadsForChart:
                    return new LoadEventsForChartStrategy(reader);
                case LoadStrategyType.LoadEventsForChart:
                    return new LoadEventsForChartStrategy(reader, false);
                case LoadStrategyType.LoadEventsAndPayloads:
                    return new LoadEventsWithoutDensitiesStrategy(reader);
                case LoadStrategyType.LoadOnlyEvents:
                    return new LoadEventsWithoutDensitiesStrategy(reader, false);
                default:
                    throw new ArgumentOutOfRangeException(nameof(LoadStrategyType), loadStrategyType, "Ünknown load strategy");

            }
        }
    }
}
