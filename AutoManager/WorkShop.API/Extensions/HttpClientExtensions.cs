using WorkShop.API.Services;

namespace WorkShop.API.Extensions;

public static class HttpClientExtensions
{
    public static IServiceCollection AddCatalogHttpClient(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var partsCatalogUrl =
            configuration["ExternalServices:PartsCatalogUrl"]
            ?? throw new InvalidOperationException(
                "A configuração ExternalServices:PartsCatalogUrl não foi encontrada.");

        services.AddHttpClient<CatalogoPecasService>(client =>
        {
            client.BaseAddress = new Uri(partsCatalogUrl);
            client.Timeout = TimeSpan.FromSeconds(10);
        });

        return services;
    }
}