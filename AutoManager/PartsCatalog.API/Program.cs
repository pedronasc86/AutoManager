using Microsoft.EntityFrameworkCore;
using PartsCatalog.API.Data;
using PartsCatalog.API.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Adicionar o DbContext
builder.Services.AddDbContext<CatalogDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Registar o Repositório de Peças para Injeção de Dependência
builder.Services.AddScoped<IPecaRepository, PecaRepository>();

// Adicionar suporte a Controllers
builder.Services.AddControllers();

// Configurar o Swagger (para documentação e testes no browser)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configurar o pipeline HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

// Mapear os endpoints dos teus Controllers
app.MapControllers();

app.Run();