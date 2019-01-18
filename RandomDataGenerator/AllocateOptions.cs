using CommandLine;
using System;


namespace RandomDataGenerator
{
    [Verb("allocate", HelpText = "Starts allocation")]
    class AllocateOptions
    {
        [Option('d', "duration", Required = false, HelpText = "Specifies how long we should watch for Garbage Collections. Eligible only for non-fake mode.")]
        public TimeSpan Duration { get; set; }

        [Option('m', "mode", Required = false, HelpText = "Specifies the mode for test allocations. Allowed values: hard, light. Eligible only for non-fake mode.")]
        public string AllocationModeRaw { get; set; }

        public AllocationMode AllocationMode
        {
            get
            {
                if (Enum.TryParse(AllocationModeRaw, true, out AllocationMode parsed))
                    return parsed;
                return default;
            }
        }
    }
}
