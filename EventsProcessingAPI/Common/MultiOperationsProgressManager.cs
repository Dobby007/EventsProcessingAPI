using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace EventsProcessingAPI.Common
{
    sealed class MultiOperationsProgressManager
    {
        private readonly IProgress<int> _progressHandler;
        private readonly List<SingleOperationProgress> _operations = new List<SingleOperationProgress>();
        private SpinLock _lock = new SpinLock();
        private volatile int _totalProgress = 0;

        public MultiOperationsProgressManager(IProgress<int> progressHandler)
        {
            _progressHandler = progressHandler;
        }

        public IProgress<int> AddOperation()
        {
            var operationProgress = new SingleOperationProgress(this);
            _operations.Add(operationProgress);
            return operationProgress;
        }
        

        private void ReportTotalProgress()
        {
            var totalProgress = _operations.Sum(op => op.Value) / _operations.Count;
            if (_totalProgress > totalProgress)
            {
                throw new InvalidOperationException("Current total progress is greater than the new one");
            }
            _totalProgress = totalProgress;
            _progressHandler?.Report(totalProgress);
        }

        private void BeginUpdate(ref bool lockTaken)
        {
            _lock.Enter(ref lockTaken);
        }

        private void EndUpdate()
        {
            _lock.Exit();
        }


        private class SingleOperationProgress : IProgress<int>
        {
            private readonly MultiOperationsProgressManager _parent;

            public SingleOperationProgress(MultiOperationsProgressManager parent)
            {
                _parent = parent;
            }

            public volatile int Value;

            public void Report(int value)
            {
                bool lockTaken = false;
                try
                {
                    _parent.BeginUpdate(ref lockTaken);
                    if (value > Value) {
                        Value = value;
                        _parent.ReportTotalProgress();
                    }
                }
                finally
                {
                    if (lockTaken)
                        _parent.EndUpdate();
                }
            }
        }
    }
}
 