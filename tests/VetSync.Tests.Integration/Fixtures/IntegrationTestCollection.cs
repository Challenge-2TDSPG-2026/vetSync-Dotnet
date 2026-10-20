using Xunit;

namespace VetSync.Tests.Integration.Fixtures;

/// <summary>Collection Fixture: uma única instância da API é compartilhada entre todas as classes de teste.</summary>
[CollectionDefinition(Name)]
public class IntegrationTestCollection : ICollectionFixture<CustomWebApplicationFactory>
{
    public const string Name = "Integration Tests";
}
