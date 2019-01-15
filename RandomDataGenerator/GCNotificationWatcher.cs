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
        public event Action OnGarbageCollectionStarted;
        public event Action OnGarbageCollectionEnded;

        public void Start()
        {
            if (disposedValue)
                throw new ObjectDisposedException(nameof(GCNotificationWatcher));

            if (_isStarted)
                return;

            _isStarted = true;
            _watcherThread = new Thread(WatchForGC);
            _watcherThread.IsBackground = true;
            _watcherThread.Start();
}
        
        private void WatchForGC()
        {

            using (var userSession = new TraceEventSession("ObserveGCCollections"))
            {
                // enable the CLR provider with default keywords (minus the rundown CLR events)
                userSession.EnableProvider(ClrTraceEventParser.ProviderGuid, TraceEventLevel.Verbose,
                    (ulong)(ClrTraceEventParser.Keywords.GC));

                userSession.Source.Clr.GCSuspendEEStop += Clr_GCSuspendEEStop;
                userSession.Source.Clr.GCRestartEEStop += Clr_GCRestartEEStop;

                _traceEventSession = userSession;

                // OK we are all set up, time to listen for events and pass them to the observers.  
                userSession.Source.Process();
            }
        }

        private void Clr_GCRestartEEStop(GCNoUserDataTraceData obj)
        {
            OnGarbageCollectionEnded?.Invoke();
        }

        private void Clr_GCSuspendEEStop(GCNoUserDataTraceData obj)
        {
            OnGarbageCollectionStarted?.Invoke();
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
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
