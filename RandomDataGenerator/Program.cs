using CommandLine;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime;
using System.Threading;


namespace RandomDataGenerator
{
    class Program
    {
        static int Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("If you need help, please type in: RandomDataGenerator --help");
                return -1;
            }


            try
            {
                var stopwatch = Stopwatch.StartNew();
                GCSettings.LatencyMode = GCLatencyMode.Interactive;

                return Parser.Default.ParseArguments<AllocateOptions, GenerateOptions>(args)
                    .MapResult(
                          (AllocateOptions opts) => StartAllocation(opts),
                          (GenerateOptions opts) => GenerateFile(opts),
                          errs => 1);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return -2;
            }
        }  
        
        private static int StartAllocation(AllocateOptions options)
        {
            Console.WriteLine($"Allocation started: duration = {options.Duration}, mode = {options.AllocationMode}");
            var spinWait = new SpinWait();
            var heavyMetal = new ObjectAllocator(options.AllocationMode);
            heavyMetal.Start();
            
            var timer = new Timer(state =>
            {
                heavyMetal.Stop();
            }, null, (long)options.Duration.TotalMilliseconds, Timeout.Infinite);

            while (heavyMetal.IsRunning)
                spinWait.SpinOnce();

            Console.WriteLine("Allocation ended");

            return 0;
        }

        private static int GenerateFile(GenerateOptions options)
        {
            Console.WriteLine($"Generation started: fake mode = {options.Fake}");
            if (options.Fake)
            {
                var generator = new FakeDataGenerator(options.File, options.DesiredEventsCount, options.MaxInterval, options.MaxDuration);
                generator.GenerateFile();
            }
            else
            {
                var generator = new GcEventsGenerator(options);
                generator.GenerateFile();
            }

            Console.WriteLine("\nGeneration ended");
            return 0;
        }
    }
}
