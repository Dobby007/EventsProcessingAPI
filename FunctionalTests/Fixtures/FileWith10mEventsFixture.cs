using RandomDataGenerator;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime;
using System.Text;

namespace FunctionalTests.Fixtures
{
    public class FileWith10mEventsFixture
    {
        public string FileName { get; } = "events_10m.bin";

        public FileWith10mEventsFixture()
        {
            if (File.Exists(FileName))
                return;

            GCSettings.LatencyMode = GCLatencyMode.Batch;
            var generator = new Generator(FileName, TimeSpan.Parse("00:01:00"));
            generator.GenerateFile(10_000_000);
        }
    }
}
