using System;
using System.Collections.Generic;
using System.Text;

namespace FunctionalTests
{
    public class AssertionInfo
    {
        public long SegmentSize { get; set; }
        public double Actual { get; set; }
        public double Expected { get; set; }
        public double Index { get; set; }
    }
}
