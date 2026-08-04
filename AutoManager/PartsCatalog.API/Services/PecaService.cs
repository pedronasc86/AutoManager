using PartsCatalog.API.Data;
using PartsCatalog.API.Models;

namespace PartsCatalog.API.Services
{
    public class PecaService : IPecaService
    {
        private readonly CatalogDbContext _context;

        public PecaService(CatalogDbContext context)
        {
            _context = context;
        }

        public IQueryable<Peca> GetPartsQuery()
        {
            return _context.Pecas.AsQueryable();
        }
    }
}