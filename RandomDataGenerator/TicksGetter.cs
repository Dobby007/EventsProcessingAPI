using System;
using System.Runtime.InteropServices;

namespace RandomDataGenerator
{
    class TicksGetter
    {
        private readonly long _frequency;
        private const double TicksPerSecond = 10_000_000;

        public TicksGetter()
        {
            _frequency = GetFrequency();
        }

        public long Peek()
        {
            return (long)(GetValue() / (double)_frequency * TicksPerSecond);
        }

        private long GetFrequency()
        {
            long ret = 0;
            if (QueryPerformanceFrequency(ref ret) == 0)
                throw new NotSupportedException(
                   "Error while querying "
                   + "the performance counter frequency.");
            return ret;
        }

        private long GetValue()
        {
            long ret = 0;
            if (QueryPerformanceCounter(ref ret) == 0)
                throw new NotSupportedException(
                   "Error while querying "
                   + "the high-resolution performance counter.");
            return ret;
        }

        [DllImport("kernel32.dll")]
        extern static int QueryPerformanceCounter(ref long x);

        [DllImport("kernel32.dll")]
        extern static int QueryPerformanceFrequency(ref long x);

    }
}
