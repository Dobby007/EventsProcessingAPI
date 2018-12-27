using System;
using System.Collections.Generic;
using System.Text;

namespace EventsUtility
{
    class ConsoleProgress : IProgress<int>
    {
        public void Report(int value)
        {
            Console.Write("\rDone: {0}", value);
        }
    }
}
