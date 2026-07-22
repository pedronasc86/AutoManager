using Microsoft.EntityFrameworkCore;
using PartsCatalog.API.Models;

namespace PartsCatalog.API.Data
{
    public class CatalogDbContext : DbContext
    {
        public CatalogDbContext(DbContextOptions<CatalogDbContext> options) : base(options)
        {
        }

        // Isto vai criar a tabela "Pecas" na Base de Dados
        public DbSet<Peca> Pecas { get; set; }
    }
}