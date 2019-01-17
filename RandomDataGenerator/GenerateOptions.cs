using CommandLine;
using System;


namespace RandomDataGenerator
{
    [Verb("generate", HelpText = "Generates event file")]
    class GenerateOptions
    {
        [Option(Required = false, HelpText = "Generate fake events")]
        public bool Fake { get; set; }

        [Option('f', "file", Required = true, HelpText = "Path to the generated file")]
        public string File { get; set; }

        [Option(Required = false, HelpText = "Max interval between stop and start events. Eligible only for fake mode.")]
        public uint MaxInterval { get; set; }

        [Option(Required = false, HelpText = "Max interval between start and stop events. Eligible only for fake mode.")]
        public uint MaxDuration { get; set; }

        [Option('d', "duration", Required = false, HelpText = "Specifies how long we should watch for Garbage Collections. Eligible only for non-fake mode.")]
        public TimeSpan Duration { get; set; }

        [Option('m', "mode", Required = false, HelpText = "Specifies the mode for test allocations. Allowed values: hard, light. Permitted only for non-fake mode.")]
        public AllocationMode AllocationMode { get; set; }

        [Option('n', Required = false, HelpText = "Specifies desired number of events that should be written to the file. In non-fake mode extrapollation will be done if there are not enough events catched.")]
        public int DesiredEventsCount { get; set; }
    }
}
