using EventsProcessingAPI.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventsChart
{
    static class TimeUnitFactors
    {
        private static readonly Dictionary<TimeUnit, double[]> _factors = new Dictionary<TimeUnit, double[]>();

        static TimeUnitFactors()
        {
            BuildTable();
        }

        private static void BuildTable()
        {
            foreach (var unit in TimeUnitHelpers.GetAllTimeUnits())
            {
                if (TimeUnitHelpers.TryGetNextTimeUnit(unit, out TimeUnit nextTimeUnit))
                {
                    _factors.Add(unit, Factors(nextTimeUnit.GetTimeUnitDuration() / unit.GetTimeUnitDuration()).ToArray());
                }
            }
        }

        public static bool TryGetBestTimeFactor(double time, TimeUnit unit, out double bestFactor)
        {
            var factors = _factors[unit];
            int index = Array.BinarySearch(factors, time);
            if (index < 0)
            {
                index = ~index;
            }
            if (index < factors.Length && index >= 0)
            {

                bestFactor = factors[index];
                return true;
            }

            bestFactor = 0;
            return false;
        }

        private static List<double> Factors(double number)
        {
            List<double> factors = new List<double>();
            long max = (long)Math.Sqrt(number);
            for (long factor = 1; factor <= max; ++factor)
            { 
                if (number % factor == 0)
                {
                    factors.Add(factor);
                    if (factor != number / factor)
                    {
                        factors.Add(number / factor);
                    }
                }
            }

            factors.Sort();
            return factors;
        }
    }
}
