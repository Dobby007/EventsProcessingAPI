using EventsDomain;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace RandomDataGenerator
{
    class FileWriter : IDisposable
    {
        private readonly BinaryWriter _writer;

        public FileWriter(string filename, bool append = false)
        {
            _writer = new BinaryWriter(new FileStream(filename, append ? FileMode.Append : FileMode.Create, FileAccess.Write));
        }

        public void WriteEventsNumber(int eventsNumber)
        {
            _writer.Write((uint)eventsNumber);
        }
        

        public void WriteEvent(in RealEvent ev, in Payload payload)
        {
            _writer.Write((byte)ev.EventType);
            _writer.Write(ev.Ticks);
            _writer.Write(payload.First);
            _writer.Write(payload.Second);
            _writer.Write(payload.Third);
            _writer.Write(payload.Fourth);
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _writer.Dispose();
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
