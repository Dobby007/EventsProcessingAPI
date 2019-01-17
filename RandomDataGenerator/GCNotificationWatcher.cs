using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing.Parsers;
using Microsoft.Diagnostics.Tracing.Parsers.Clr;
using Microsoft.Diagnostics.Tracing.Session;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reactive.Linq;
using System.Threading;

namespace RandomDataGenerator
{
    class GCNotificationWatcher : IDisposable
    {
        private volatile bool _isStarted = false;
        private Thread _watcherThread;
        private volatile TraceEventSession _traceEventSession;
        public event Action<long> OnGarbageCollectionStarted;
        public event Action<long> OnGarbageCollectionEnded;
        private int _targetProcessId = -1;

        public void Start(int targetProcessId)
        {
            if (disposedValue)
                throw new ObjectDisposedException(nameof(GCNotificationWatcher));
            if (_isStarted)
                return;

            _targetProcessId = targetProcessId;
            _isStarted = true;
            _watcherThread = new Thread(WatchForGC);
            _watcherThread.IsBackground = true;
            _watcherThread.Start();
        }
        
        private void WatchForGC()
        {

            using (_traceEventSession = new TraceEventSession("ObserveGCCollections"))
            {
                // enable CLR provider to capture only GC events
                _traceEventSession.EnableProvider(ClrTraceEventParser.ProviderGuid, TraceEventLevel.Verbose,
                    (ulong)ClrTraceEventParser.Keywords.GC);

                // initialize observers
                _traceEventSession.Source.Clr.GCSuspendEEStop += Clr_GCSuspendEEStop;
                _traceEventSession.Source.Clr.GCRestartEEStop += Clr_GCRestartEEStop;
                
                // start listening to incoming events from CLR
                _traceEventSession.Source.Process();
                
            }
        }

        private void Clr_GCRestartEEStop(GCNoUserDataTraceData obj)
        {
            if (_targetProcessId == obj.ProcessID)
                OnGarbageCollectionEnded?.Invoke((long)(obj.TimeStampRelativeMSec * 10_000));
        }

        private void Clr_GCSuspendEEStop(GCNoUserDataTraceData obj)
        {
            if (_targetProcessId == obj.ProcessID)
                OnGarbageCollectionStarted?.Invoke((long)(obj.TimeStampRelativeMSec * 10_000));
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _traceEventSession?.Stop();
                    _isStarted = false;
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
