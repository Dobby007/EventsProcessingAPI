using CommandLine;
using System;


namespace RandomDataGenerator
{
    [Verb("allocate", HelpText = "Starts allocation")]
    class AllocateOptions
    {
        [Option(Required = false, HelpText = "Specifies how long we should watch for Garbage Collections. Eligible only for non-fake mode.")]
        public TimeSpan Duration { get; set; }

        [Option(Required = false, HelpText = "Specifies the mode for test allocations. Allowed values: hard, light. Eligible only for non-fake mode.")]
        public AllocationMode AllocationMode { get; set; }
    }
}
