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
        static void Main(string[] args)
        {
            if (args.Length < 5)
            {
                Console.WriteLine("Usage: dotnet RandomDataGenerator.dll .....");
                Console.WriteLine("Examples: " +
                    "\ndotnet RandomDataGenerator.dll ..\\..\\..\\..\\data\\events.bin false 00:03:00 1000000 hard" +
                    "\ndotnet RandomDataGenerator.dll ..\\..\\..\\..\\data\\events.bin false 00:03:00 1000000 light" +
                    "\ndotnet RandomDataGenerator.dll ..\\..\\..\\..\\data\\events.bin true 10000 1000 1000000");
                return;
            }


            try
            {
                var stopwatch = Stopwatch.StartNew();
                GCSettings.LatencyMode = GCLatencyMode.Interactive;

                return Parser.Default.ParseArguments<AllocateOptions, GenerateOptions>(args)
                    .MapResult(
                          (AllocateOptions opts) => RunAddAndReturnExitCode(opts),
                          (GenerateOptions opts) => RunCommitAndReturnExitCode(opts),
                          errs => 1);

                

                if (args.Length >= 5)
                {
                    var heavyMetal = new ObjectAllocator(_allocationMode);
                    heavyMetal.Start();

                    var timer = new Timer(state =>
                    {
                        heavyMetal.Stop();
                    }, null, (long)_interval.TotalMilliseconds, Timeout.Infinite);
                }

                bool isFakeMode = bool.Parse(args[1]);
                if (isFakeMode)
                {
                    var maxEventsInterval = uint.Parse(args[2]);
                    var maxDuration = uint.Parse(args[3]);
                    var generator = new FakeDataGenerator(args[0], maxEventsInterval, maxDuration);
                    generator.GenerateFile(int.Parse(args[4]));
                }
                else
                {
                    var generator = new GcEventsGenerator(args[0], TimeSpan.Parse(args[2]), (AllocationMode)Enum.Parse(typeof(AllocationMode), args[4], true));
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

    [Verb("allocate", HelpText = "Starts allocation")]
    class AllocateOptions : BaseOptions
    {
        public AllocationMode AllocationMode { get; set; }
    }

    [Verb("generate", HelpText = "Generates event file")]
    class GenerateOptions : BaseOptions
    {
        public bool FakeData { get; set; }
    }

    abstract class BaseOptions
    {
        public string File { get; set; }
        
        public TimeSpan Duration { get; set; }

        public uint MaxEventsInterval { get; set; }

        public uint MaxDuration { get; set; }
    }
}
