using WorkShop.API.Extensions;
using WorkShop.API.HealthChecks;
using WorkShop.API.Services.Auth;
using Microsoft.EntityFrameworkCore;
using WorkShop.API.Data;

namespace WorkShop.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // 1. Configurar DbContext com fallback de segurança para a Migration
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                ?? "Server=(localdb)\\mssqllocaldb;Database=AutoManager_WorkshopDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";

            builder.Services.AddDbContext<WorkshopContext>(options =>
                options.UseSqlServer(connectionString));

            // 2. Controladores e Swagger
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // 3. Serviços do Projeto
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddScoped<IUserContextService, UserContextService>();
            builder.Services.AddCustomAuthentication(builder.Configuration);
            builder.Services.AddCatalogHttpClient(builder.Configuration);

            builder.Services.AddAuthorization();

            builder.Services.AddHealthChecks()
                .AddCheck<PartsCatalogHealthCheck>("parts_catalog_health_check");

            var app = builder.Build();

            // 4. Pipeline de Pedidos (HTTP Pipeline)
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseCustomMiddlewares();

            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();
            app.MapHealthChecks("/health");

            app.Run();
        }
    }
}