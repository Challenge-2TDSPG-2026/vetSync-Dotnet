using VetApi.Services;
using Xunit;

namespace VetApi.Tests.Unit.Services;

public class GeoLocationServiceFixture
{
    public IGeoLocationService Service { get; } = new GeoLocationService();
}

[CollectionDefinition("GeoLocationService Collection")]
public class GeoLocationServiceCollection : ICollectionFixture<GeoLocationServiceFixture>
{
}
