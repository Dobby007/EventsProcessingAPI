using System;
using System.Collections.Generic;
using System.Text;
using UnitTests.Common;

namespace Xunit
{
    public partial class Assert
    {
        private static DensityEqualityComparer _densityEqualityComparer = new DensityEqualityComparer();
        public static void DensitiesEqual(double expected, double actual)
        {
            Equal(expected, actual, _densityEqualityComparer);
        }
    }
}
