using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using EventsDomain;

namespace EventsProcessingAPI.Common.Pipeline
{
    sealed class EmptyPipeline : IProcess
    {
        public void Complete(BucketContainer container)
        {
            
        }

        public void ProcessBucket(ArraySegment<Bucket> buckets, int index)
        {
            
        }
    }
}
