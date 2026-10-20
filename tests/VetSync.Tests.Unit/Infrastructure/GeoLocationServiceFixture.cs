using VetSync.Application.Abstractions;
using VetSync.Infrastructure.Services;
using Xunit;

namespace VetSync.Tests.Unit.Infrastructure;

public class GeoLocationServiceFixture
{
    public IGeoLocationService Service { get; } = new GeoLocationService();
}

[CollectionDefinition("GeoLocationService Collection")]
public class GeoLocationServiceCollection : ICollectionFixture<GeoLocationServiceFixture>
{
}
