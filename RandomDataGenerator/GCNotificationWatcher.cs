using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace RandomDataGenerator
{
    class GCNotificationWatcher : IDisposable
    {
        private volatile bool _isStarted = false;
        private Thread _watcherThread;

        public event Action OnGarbageCollectionStarted;
        public event Action OnGarbageCollectionEnded;

        public void Start()
        {
            if (disposedValue)
                throw new ObjectDisposedException(nameof(GCNotificationWatcher));

            if (_isStarted)
                return;

            try
            {
                GC.RegisterForFullGCNotification(10, 10);
            }
            catch (InvalidOperationException invalidOp)
            {
                Console.WriteLine("GC Notifications are not supported while concurrent GC is enabled.\n"
                    + invalidOp.Message);
            }

            _isStarted = true;
            _watcherThread = new Thread(WatchForGC);
            _watcherThread.IsBackground = true;
            _watcherThread.Start();
}
        
        private void WatchForGC()
        {
            while (_isStarted)
            {
                // Check for a notification of an approaching collection.
                GCNotificationStatus s = GC.WaitForFullGCApproach();

                if (!_isStarted)
                    return;

                if (s == GCNotificationStatus.Succeeded)
                {
                    //Console.WriteLine("GC Notification raised.");
                    OnGarbageCollectionStarted?.Invoke();
                }
                else if (s == GCNotificationStatus.Canceled)
                {
                    Console.WriteLine("GC Notification cancelled.");
                    break;
                }
                else
                {
                    // This can occur if a timeout period
                    // is specified for WaitForFullGCApproach(Timeout) 
                    // or WaitForFullGCComplete(Timeout)  
                    // and the time out period has elapsed. 
                    Console.WriteLine("GC Notification not applicable.");
                    break;
                }

                // Check for a notification of a completed collection.
                GCNotificationStatus status = GC.WaitForFullGCComplete();
                if (status == GCNotificationStatus.Succeeded)
                {
                    //Console.WriteLine("GC Notification raised.");
                    OnGarbageCollectionEnded?.Invoke();
                }
                else if (status == GCNotificationStatus.Canceled)
                {
                    Console.WriteLine("GC Notification cancelled.");
                    break;
                }
                else
                {
                    // Could be a time out.
                    Console.WriteLine("GC Notification not applicable.");
                    break;
                }
            }

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
