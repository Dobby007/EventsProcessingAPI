using System;
using System.Collections.Generic;
using System.Text;

namespace PerfomanceTest
{
    class ConsoleProgress : IProgress<int>
    {
        public void Report(int value)
        {
            Console.Write("Done: {0}\r", value);
        }
    }
}
