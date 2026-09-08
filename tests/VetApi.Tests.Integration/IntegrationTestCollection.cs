using Xunit;

namespace VetApi.Tests.Integration;

[CollectionDefinition("Integration Tests")]
public class IntegrationTestCollection : ICollectionFixture<CustomWebApplicationFactory>
{
}
