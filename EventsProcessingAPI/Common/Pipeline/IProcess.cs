using EventsDomain;
using System;

namespace EventsProcessingAPI.Common.Pipeline
{
    /// <summary>
    /// Interface that defines process step in the pipeline
    /// </summary>
    interface IProcess
    {
        /// <summary>
        /// Processes buckets one-by-one
        /// </summary>
        /// <param name="buckets">Array of buckets</param>
        /// <param name="index">Index of bucket to process</param>
        void ProcessBucket(ArraySegment<Bucket> buckets, int index);

        /// <summary>
        /// Completes started process and flushes all the information that was calculated so far
        /// </summary>
        /// <param name="container">Bucket container</param>
        void Complete(BucketContainer container);
    }
}
