using CommandLine;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace RandomDataGenerator
{
    internal static class AllocationManager
    {
        public static Process StartProcess(AllocateOptions options)
        {
            var filePath = Assembly.GetEntryAssembly().Location;
            var argsStr = Parser.Default.FormatCommandLine(options);
            return Process.Start(filePath, argsStr);
        }
    }
}
