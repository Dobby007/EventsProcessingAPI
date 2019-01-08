using System;
using System.Collections.Generic;
using System.Text;

namespace UnitTests.Common
{
    class DensityEqualityComparer : IEqualityComparer<double>
    {
        public bool Equals(double x, double y)
        {
            return Math.Round(x, 7) == Math.Round(y, 7);
        }

        public int GetHashCode(double obj)
        {
            return obj.GetHashCode();
        }
    }
}
