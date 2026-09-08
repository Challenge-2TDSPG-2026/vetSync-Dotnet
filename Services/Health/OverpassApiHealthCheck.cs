using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace VetApi.Services.Health;

public class OverpassApiHealthCheck : IHealthCheck
{
    private const string StatusUrl = "https://overpass-api.de/api/status";

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<OverpassApiHealthCheck> _logger;

    public OverpassApiHealthCheck(IHttpClientFactory httpClientFactory, ILogger<OverpassApiHealthCheck> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("OverpassHealthCheck");

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(5));

            var response = await client.GetAsync(StatusUrl, cts.Token);

            return response.IsSuccessStatusCode
                ? HealthCheckResult.Healthy("Overpass API (OpenStreetMap) respondendo normalmente.")
                : HealthCheckResult.Degraded($"Overpass API respondeu com status {(int)response.StatusCode}.");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Falha ao verificar disponibilidade da Overpass API");

            return HealthCheckResult.Degraded(
                "Overpass API (OpenStreetMap) indisponível no momento — a busca de clínicas próximas pode falhar.",
                ex);
        }
    }
}
