using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalProject.Tests.Helpers
{
    [CollectionDefinition("Integration Tests")]
    public class IntegrationTestCollection
        : ICollectionFixture<CustomWebApplicationFactory>
    {
    }
}
