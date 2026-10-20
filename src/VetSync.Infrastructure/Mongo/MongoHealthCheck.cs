using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace VetSync.Infrastructure.Mongo;

public class MongoHealthCheck : IHealthCheck
{
    private readonly IMongoClient _client;
    private readonly MongoSettings _settings;

    public MongoHealthCheck(IMongoClient client, IOptions<MongoSettings> options)
    {
        _client = client;
        _settings = options.Value;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(_settings.ServerSelectionTimeoutSeconds + 1));

        try
        {
            await _client.GetDatabase(_settings.Database)
                .RunCommandAsync<BsonDocument>(new BsonDocument("ping", 1), cancellationToken: timeout.Token);
            return HealthCheckResult.Healthy("MongoDB respondeu ao ping");
        }
        catch (Exception ex)
        {
            return new HealthCheckResult(context.Registration.FailureStatus, "MongoDB indisponível", ex);
        }
    }
}
