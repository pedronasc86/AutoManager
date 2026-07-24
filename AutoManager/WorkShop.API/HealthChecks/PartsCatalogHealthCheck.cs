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

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            // Chamada ao método correto do CatalogoPecasService

            var (temStock, preco, mensagemErro) = await _catalogClient.VerificarStockEObterPrecoAsync(1, 1);

            return HealthCheckResult.Healthy("PartsCatalog.API está operacional.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("PartsCatalog.API inacessível.", ex);
        }
    }
}
