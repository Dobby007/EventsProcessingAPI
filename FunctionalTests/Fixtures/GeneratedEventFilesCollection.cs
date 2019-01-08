using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace FunctionalTests.Fixtures
{
    [CollectionDefinition(CollectionFixtures.RandomEventFilesCollection)]
    public class GeneratedEventFilesCollection  
        : ICollectionFixture<FileWith100kEventsFixture>, ICollectionFixture<FileWith500kEventsFixture>,
          ICollectionFixture<FileWith1mEventsFixture>, ICollectionFixture<FileWith10mEventsFixture>
    {
    }
}
