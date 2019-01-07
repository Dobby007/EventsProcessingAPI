using EventsDomain;
using EventsProcessingAPI.Common;
using EventsProcessingAPI.Common.Pipeline;
using EventsProcessingAPI.DataRead;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EventsProcessingAPI.Load
{
    internal class ParallelEventsProcessor : IEventsProcessor
    {
        private readonly IProgress<int> _progressHandler;
        private readonly IProcess _eventProcessor;

        public ParallelEventsProcessor(IProcess eventProcessor, IProgress<int> progressHandler = null)
        {
            _eventProcessor = eventProcessor;
            _progressHandler = progressHandler;
        }


        public Task<BucketContainer> ProcessEventsAsync(IEventReader reader, bool enablePayloads)
        {
            var progressManager = new MultiOperationsProgressManager(_progressHandler);

            IProgress<int> operationProgress = progressManager.AddOperation();
            var readingTask = reader.StartReadingEvents(
                enablePayloads,
                operationProgress
            );

            operationProgress = progressManager.AddOperation();
            var processTask = StartProcessingEventsWithPipeline(
                reader,
                operationProgress
            );

            return Task.WhenAll(readingTask, processTask)
                .ContinueWith(
                    task => {
                        if (task.IsFaulted)
                        {
                            var tcs = new TaskCompletionSource<BucketContainer>();
                            tcs.SetException(task.Exception);
                            return tcs.Task;
                        }
                        return processTask;
                        
                    }
                ).Unwrap();
        }


        private Task<BucketContainer> StartProcessingEventsWithPipeline(
            IEventReader reader, IProgress<int> progress)
        {
            var tcs = new TaskCompletionSource<BucketContainer>();
            var thread = new Thread(() =>
            {
                try
                {
                    var bucketsList = new ResizableArray<Bucket>();
                    double processedEventsCount = 0;
                    int bucketCount = 0;
                    foreach (var bucket in reader.GetAllBuckets())
                    {
                        processedEventsCount += bucket.Events.Length;
                        bucketsList.Add(bucket);
                        _eventProcessor.ProcessBucket(bucketsList.GetArraySegment(), bucketCount);

                        if (reader.TotalEventsCount > 0)
                        {
                            progress.Report((int)Math.Min(processedEventsCount / reader.TotalEventsCount * 100, 100));
                        }
                        bucketCount++;
                    }
                    progress.Report(100);
                    var buckets = bucketsList.ToArray();
                    bucketsList = null;
                    var container = new BucketContainer(buckets, reader.FirstTimestamp);
                    _eventProcessor.Complete(container);
                    tcs.SetResult(container);
                }
                catch (TaskCanceledException)
                {
                    tcs.SetCanceled();
                }
                catch (Exception exc)
                {
                    tcs.SetException(exc);
                }
            });
            thread.Start();

            return tcs.Task;
        }
    }
}
