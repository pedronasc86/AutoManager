using WorkShop.API.Services.Integration;

namespace WorkShop.API.Extensions;

public static class HttpClientExtensions
{
    public static IServiceCollection AddCatalogHttpClient(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpClient<ICatalogServiceClient, CatalogServiceClient>(client =>
        {
            client.BaseAddress = new Uri(configuration["ExternalServices:PartsCatalogUrl"] ?? "https://localhost:5001");
        });

        return services;
    }
}
