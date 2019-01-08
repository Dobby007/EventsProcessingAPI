using RandomDataGenerator;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime;
using System.Text;

namespace FunctionalTests.Fixtures
{
    public class FileWith500kEventsFixture
    {
        public string FileName { get; } = "events_500k.bin";

        public FileWith500kEventsFixture()
        {
            if (File.Exists(FileName))
                return;

            var generator = new FakeDataGenerator(FileName);
            generator.GenerateFile(500_000);
        }
    }
}
