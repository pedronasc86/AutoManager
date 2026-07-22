using Microsoft.EntityFrameworkCore;
using PartsCatalog.API.Data;
using PartsCatalog.API.Models;

namespace PartsCatalog.API.Repositories
{
    public class PecaRepository : IPecaRepository
    {
        private readonly CatalogDbContext _context;

        public PecaRepository(CatalogDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Peca>> ObterTodasAsync()
        {
            return await _context.Pecas.AsNoTracking().ToListAsync();
        }

        public async Task<Peca?> ObterPorIdAsync(Guid id)
        {
            return await _context.Pecas.FindAsync(id);
        }

        public async Task CriarAsync(Peca peca)
        {
            await _context.Pecas.AddAsync(peca);
            await _context.SaveChangesAsync();
        }

        public async Task AtualizarAsync(Peca peca)
        {
            _context.Pecas.Update(peca);
            await _context.SaveChangesAsync();
        }

        public async Task RemoverAsync(Guid id)
        {
            var peca = await ObterPorIdAsync(id);
            if (peca != null)
            {
                _context.Pecas.Remove(peca);
                await _context.SaveChangesAsync();
            }
        }
    }
}