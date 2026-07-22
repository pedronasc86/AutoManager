using System.Net.Http.Json;
using WorkShop.API.DTOs;

namespace WorkShop.API.Services.Integration;

public class CatalogServiceClient : ICatalogServiceClient
{
    private readonly HttpClient _httpClient;

    public CatalogServiceClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<PartAvailabilityResponseDto?> CheckPartAvailabilityAsync(int partId, int quantity)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/parts/{partId}/availability?quantity={quantity}");

            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<PartAvailabilityResponseDto>();
        }
        catch (HttpRequestException)
        {
            return null; // Caso o PartsCatalog.API esteja offline
        }
    }
}
