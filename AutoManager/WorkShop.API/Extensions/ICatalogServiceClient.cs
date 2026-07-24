namespace WorkShop.API.Services.Integration
{
    public interface ICatalogServiceClient
    {
        Task<bool> CheckPartAvailabilityAsync(int partId, int quantity);
    }
}