using RandomDataGenerator;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            
            
            var generator = new FakeDataGenerator(FileName, 100_000);
            generator.GenerateFile();


        }
    }
}
