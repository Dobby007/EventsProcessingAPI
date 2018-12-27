using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace EventsProcessingAPI.Exceptions
{
    public class RangeNotFoundException : Exception
    {
        public RangeNotFoundException()
        {
        }

        public RangeNotFoundException(string message) : base(message)
        {
        }

        public RangeNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected RangeNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
