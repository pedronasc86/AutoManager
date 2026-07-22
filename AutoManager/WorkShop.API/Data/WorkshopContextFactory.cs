using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace WorkShop.API.Data
{
    public class WorkshopContextFactory : IDesignTimeDbContextFactory<WorkshopContext>
    {
        public WorkshopContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<WorkshopContext>();

            // Connection string direta para o EF Tools usar na geração de migrations
            var connectionString = "Server=(localdb)\\mssqllocaldb;Database=AutoManager_WorkshopDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";

            optionsBuilder.UseSqlServer(connectionString);

            return new WorkshopContext(optionsBuilder.Options);
        }
    }
}