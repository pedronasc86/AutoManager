using PartsCatalog.API.Models;

namespace PartsCatalog.API.Services
{
    public interface IPecaService
    {
        IQueryable<Peca> GetPartsQuery();
    }
}