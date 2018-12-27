using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using EventsDomain;

namespace EventsProcessingAPI.Common.Pipeline
{
    class ProcessPipeline : IProcess, ICollection<IProcess>
    {
        private readonly List<IProcess> _processes = new List<IProcess>();

        public int Count => _processes.Count;

        public bool IsReadOnly => ((ICollection<IProcess>)_processes).IsReadOnly;

        public void Add(IProcess process)
        {
            _processes.Add(process);
        }

        public void Clear()
        {
            _processes.Clear();
        }

        public void Complete(BucketContainer container)
        {
            foreach (var process in _processes)
            {
                process.Complete(container);
            }
        }

        public bool Contains(IProcess item)
        {
            return _processes.Contains(item);
        }

        public void CopyTo(IProcess[] array, int arrayIndex)
        {
            _processes.CopyTo(array, arrayIndex);
        }

        public IEnumerator<IProcess> GetEnumerator()
        {
            return ((ICollection<IProcess>)_processes).GetEnumerator();
        }

        public void ProcessBucket(ArraySegment<Bucket> buckets, int index)
        {
            foreach (var process in _processes)
            {
                process.ProcessBucket(buckets, index);
            }
        }

        public bool Remove(IProcess item)
        {
            return _processes.Remove(item);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((ICollection<IProcess>)_processes).GetEnumerator();
        }
    }
}
