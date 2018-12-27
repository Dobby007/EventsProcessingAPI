using System;
using System.Collections.Generic;
using System.Text;

namespace EventsDomain
{
    public readonly struct Payload
    {
        public readonly long First;
        public readonly long Second;
        public readonly long Third;
        public readonly long Fourth;

        public Payload(long first, long second, long third, long fourth)
        {
            First = first;
            Second = second;
            Third = third;
            Fourth = fourth;
        }
    }
}
