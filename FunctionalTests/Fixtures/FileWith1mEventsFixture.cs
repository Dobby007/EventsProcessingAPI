using RandomDataGenerator;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime;
using System.Text;

namespace FunctionalTests.Fixtures
{
    public class FileWith1mEventsFixture
    {
        public string FileName { get; } = "events_1m.bin";

        public FileWith1mEventsFixture()
        {
            if (File.Exists(FileName))
                return;

            var generator = new FakeDataGenerator(FileName);
            generator.GenerateFile(1_000_000);
        }
    }
}
