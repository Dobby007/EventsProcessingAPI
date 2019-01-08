using RandomDataGenerator;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime;
using System.Text;

namespace FunctionalTests.Fixtures
{
    public class FileWith100kEventsFixture
    {
        public string FileName { get; } = "events_100k.bin";

        public FileWith100kEventsFixture()
        {
            if (File.Exists(FileName))
                return;

            GCSettings.LatencyMode = GCLatencyMode.Batch;
            var generator = new Generator(FileName, TimeSpan.Parse("00:00:30"));
            generator.GenerateFile(100_000);
            
        }
    }
}
