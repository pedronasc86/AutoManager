using Microsoft.EntityFrameworkCore;
using WorkShop.API.Models;

namespace WorkShop.API.Data
{
    public class WorkshopContext : DbContext
    {
        public WorkshopContext(DbContextOptions<WorkshopContext> options) : base(options)
        {
        }

        public DbSet<Veiculo> Veiculos { get; set; }
        public DbSet<OrdemReparacao> OrdensReparacao { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Veiculo>()
                .HasMany(v => v.OrdensReparacao)
                .WithOne(o => o.Veiculo)
                .HasForeignKey(o => o.VeiculoId);
        }
    }
}