using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PartsCatalog.API.Data;
using PartsCatalog.API.Mappings;
using PartsCatalog.API.Repositories;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// 2. Adicionar o serviço de Autenticação JWT
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings["Secret"]!))
        };
    });

builder.Services.AddAuthorization();

// Adicionar o DbContext
builder.Services.AddDbContext<CatalogDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Registar o Repositório de Peças para Injeção de Dependência
builder.Services.AddScoped<IPecaRepository, PecaRepository>();

// Adicionar suporte a Controllers
builder.Services.AddControllers();

// Regista o AutoMapper procurando perfis no projeto
builder.Services.AddAutoMapper(cfg => { }, typeof(MappingProfile));

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

app.UseAuthentication();
app.UseAuthorization();

// Mapear os endpoints dos teus Controllers
app.MapControllers();

app.Run();