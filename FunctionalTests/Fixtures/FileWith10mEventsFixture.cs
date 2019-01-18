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

            var generator = new FakeDataGenerator(FileName, 10_000_000);
            generator.GenerateFile();
        }
    }
}
