using Microsoft.Extensions.Diagnostics.HealthChecks;
using WorkShop.API.Services;

namespace WorkShop.API.HealthChecks;

public class PartsCatalogHealthCheck : IHealthCheck
{
    private readonly CatalogoPecasService _catalogClient;

    public PartsCatalogHealthCheck(CatalogoPecasService catalogClient)
    {
        _catalogClient = catalogClient;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Faz um pedido HTTP real ao endpoint público do Catálogo.
            await _catalogClient.ObterPecasAsync();

            return HealthCheckResult.Healthy(
                "PartsCatalog.API está operacional.");
        }
        catch (HttpRequestException exception)
        {
            return HealthCheckResult.Unhealthy(
                "PartsCatalog.API está indisponível.",
                exception);
        }
    }
}
