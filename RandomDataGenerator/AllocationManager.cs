using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace RandomDataGenerator
{
    public static class AllocationManager
    {
        public static void StartProcess()
        {
            var filePath = Assembly.GetEntryAssembly().Location;
            Process.Start(filePath, );
        }
    }
}
