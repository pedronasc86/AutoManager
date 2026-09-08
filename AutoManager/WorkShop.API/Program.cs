using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using WorkShop.API.Data;
using WorkShop.API.Extensions;
using WorkShop.API.HealthChecks;
using WorkShop.API.Services;
using WorkShop.API.Services.Auth;
using WorkShop.API.Services.Pedidos;

namespace WorkShop.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // 1. Configurar DbContext com fallback de segurança para a Migration
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                ?? "Server=(localdb)\\mssqllocaldb;Database=AutoManagerDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";

            builder.Services.AddDbContext<WorkshopContext>(options =>
                options.UseSqlServer(
                    connectionString
                )
            );

            // 2. Controladores e Swagger
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // 3. Serviços do Projeto
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddScoped<IUserContextService, UserContextService>();
            builder.Services.AddCustomAuthentication(builder.Configuration);
            builder.Services.Configure<JwtBearerOptions>(
                JwtBearerDefaults.AuthenticationScheme,
                options =>
                {
                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            context.Token = context.Request.Cookies["jwtToken"];
                            return Task.CompletedTask;
                        }
                    };
            });
            

            builder.Services.AddAuthorization();

            builder.Services.AddHealthChecks()
                .AddCheck<PartsCatalogHealthCheck>("parts_catalog_health_check");

            builder.Services.AddScoped<IPedidoReparacaoService, PedidoReparacaoService>();
            // Regista o HttpClient apontando para o URL da PartsCatalog.API
            //builder.Services.AddHttpClient<WorkShop.API.Services.CatalogoPecasService>(client =>
            //{
            //    client.BaseAddress = new Uri("http://localhost:5039/"); // URL onde a PartsCatalog.API corre
            //});
            builder.Services.AddCatalogHttpClient(builder.Configuration);

            // CORS corrigido para permitir o dashboard (qualquer origem local ou desenvolvimento)
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowDashboard", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                });
            });

            var app = builder.Build();

            app.UseCors("AllowDashboard");

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

            // Protege as páginas HTML conforme o perfil autenticado.
            app.Use(async (context, next) =>
            {
                var caminho = context.Request.Path.Value?.ToLowerInvariant();
                var autenticado = context.User.Identity?.IsAuthenticated == true;

                var eStaff = autenticado &&
                    (
                        context.User.IsInRole("Admin") ||
                        context.User.IsInRole("admin") ||
                        context.User.IsInRole("Mecanico") ||
                        context.User.IsInRole("mecanico")
                    );

                var eCliente = autenticado &&
                    (
                        context.User.IsInRole("Cliente") ||
                        context.User.IsInRole("cliente")
                    );

                // Só Admin e Mecânico podem abrir o dashboard administrativo.
                if (caminho == "/dashboard.html")
                {
                    if (!autenticado)
                    {
                        context.Response.Redirect(
                            "https://localhost:7194/login.html"
                        );
                        return;
                    }

                    if (!eStaff)
                    {
                        context.Response.Redirect(
                            "https://localhost:7085/dashboardCliente.html"
                        );
                        return;
                    }
                }

                // Um Admin/Mecânico não usa a página exclusiva de Cliente.
                if (caminho == "/dashboardcliente.html")
                {
                    if (!autenticado)
                    {
                        context.Response.Redirect(
                            "https://localhost:7194/login.html"
                        );
                        return;
                    }

                    if (eStaff)
                    {
                        context.Response.Redirect(
                            "https://localhost:7085/dashboard.html"
                        );
                        return;
                    }

                    if (!eCliente)
                    {
                        context.Response.Redirect(
                            "https://localhost:7194/login.html"
                        );
                        return;
                    }
                }

                await next();
            });

            app.UseDefaultFiles();
            app.UseStaticFiles();

            app.MapControllers();
            app.MapHealthChecks("/health");

            app.Run();
        }
    }
}