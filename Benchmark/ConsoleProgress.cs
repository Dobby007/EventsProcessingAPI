using System;
using System.Collections.Generic;
using System.Text;

namespace BenchmarkTest
{
    class ConsoleProgress : IProgress<int>
    {
        public void Report(int value)
        {
            if (value % 10 == 0)
                Console.WriteLine("Done: {0}", value);
        }
    }
}
