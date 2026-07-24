using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using WorkShop.API.Services;

namespace WorkShop.API.Extensions
{
    public static class HttpClientExtensions
    {
        public static IServiceCollection AddCatalogHttpClient(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddHttpClient<ICatalogoPecasService, CatalogoPecasService>(client =>
            {
                client.BaseAddress = new Uri(configuration["ExternalServices:PartsCatalogUrl"] ?? "https://localhost:5001");
            });

            return services;
        }
    }
}