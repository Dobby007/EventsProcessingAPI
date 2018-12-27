using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace EventsProcessingAPI.Exceptions
{
    public class BadEventSourceException : Exception
    {
        public BadEventSourceException()
        {
        }

        public BadEventSourceException(string message) : base(message)
        {
        }

        public BadEventSourceException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected BadEventSourceException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
