using System;
using System.Diagnostics;
using System.IO;
using System.Runtime;
using System.Threading;

namespace RandomDataGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 3)
            {
                Console.WriteLine("Usage: dotnet RandomDataGenerator.dll {pathToGeneratedFile} {approximatePeriod} {desiredEventsCount}");
                Console.WriteLine("Example: dotnet RandomDataGenerator.dll ..\\..\\..\\..\\data\\events.bin 00:03:00");
                return;
            }


            try
            {
                var stopwatch = Stopwatch.StartNew();
                GCSettings.LatencyMode = GCLatencyMode.Batch;
                var generator = new Generator(args[0], TimeSpan.Parse(args[1]));
                generator.GenerateFile(int.Parse(args[2]));
                stopwatch.Stop();
                Console.WriteLine("\nElapsed: {0}s", stopwatch.Elapsed.TotalSeconds);
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
        
        
    }
}
