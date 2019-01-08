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
            if (args.Length < 4)
            {
                Console.WriteLine("Usage: dotnet RandomDataGenerator.dll .....");
                Console.WriteLine("Examples: " + 
                    "\ndotnet RandomDataGenerator.dll ..\\..\\..\\..\\data\\events.bin false 00:03:00 hard" + 
                    "\ndotnet RandomDataGenerator.dll ..\\..\\..\\..\\data\\events.bin false 00:03:00 light" +
                    "\ndotnet RandomDataGenerator.dll ..\\..\\..\\..\\data\\events.bin true 10000 1000");
                return;
            }


            try
            {
                var stopwatch = Stopwatch.StartNew();
                GCSettings.LatencyMode = GCLatencyMode.Batch;

                bool isFakeMode = bool.Parse(args[1]);
                if (isFakeMode)
                {
                    var maxEventsInterval = uint.Parse(args[2]);
                    var maxDuration = uint.Parse(args[3]);
                    var generator = new FakeDataGenerator(args[0], maxEventsInterval, maxDuration);
                    generator.GenerateFile(int.Parse(args[2]));
                }
                else
                {
                    var generator = new GcEventsGenerator(args[0], TimeSpan.Parse(args[2]), Enum.Parse<AllocationMode>(args[4], true));
                    generator.GenerateFile(int.Parse(args[3]));
                }

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
