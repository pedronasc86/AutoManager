using Microsoft.Extensions.Diagnostics.HealthChecks;
using WorkShop.API.Services.Integration;

namespace WorkShop.API.HealthChecks;

public class PartsCatalogHealthCheck : IHealthCheck
{
    private readonly ICatalogServiceClient _catalogClient;

    public PartsCatalogHealthCheck(ICatalogServiceClient catalogClient)
    {
        _catalogClient = catalogClient;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _catalogClient.CheckPartAvailabilityAsync(1, 1);
            return HealthCheckResult.Healthy("PartsCatalog.API está operacional.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("PartsCatalog.API inacessível.", ex);
        }
    }
}
