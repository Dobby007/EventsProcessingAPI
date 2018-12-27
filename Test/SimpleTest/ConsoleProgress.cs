using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleTest
{
    class ConsoleProgress : IProgress<int>
    {
        public void Report(int value)
        {
            Console.Write("\rDone: {0}", value);
        }
    }
}
