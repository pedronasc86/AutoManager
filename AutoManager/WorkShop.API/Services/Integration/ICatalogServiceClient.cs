using WorkShop.API.DTOs;

namespace WorkShop.API.Services.Integration;

public interface ICatalogServiceClient
{
    Task<PartAvailabilityResponseDto?> CheckPartAvailabilityAsync(int partId, int quantity);
}
