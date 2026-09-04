using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.OData;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using PartsCatalog.API.Data;
using PartsCatalog.API.Mappings;
using PartsCatalog.API.Repositories;
using PartsCatalog.API.Services;
using PartsCatalog.API.Middlewares;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// 1. ADICIONAR O SERVIÇO DE AUTENTICAÇÃO JWT
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

// 2. CONFIGURAÇÃO DA BASE DE DADOS (EF CORE) E INJEÇÃO DE DEPENDÊNCIAS
builder.Services.AddDbContext<CatalogDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Registo de Repositórios e Serviços
builder.Services.AddScoped<IPecaRepository, PecaRepository>();
builder.Services.AddScoped<IPecaService, PecaService>();

// AutoMapper
builder.Services.AddAutoMapper(cfg => { }, typeof(MappingProfile));

// Controllers + Configuração do OData
builder.Services.AddControllers()
    .AddOData(options => options
        .Select()       // Permite selecionar campos
        .Filter()       // Permite filtrar ($filter)
        .OrderBy()      // Permite ordenar ($orderby)
        .Count()        // Permite contar ($count)
        .SetMaxTop(10)  // Limite máximo de registos por página ($top)
    );

// 3. CONFIGURAÇÃO DO SWAGGER (COM BOTÃO AUTHORIZE PARA O TOKEN JWT)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "PartsCatalog.API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Insira o token JWT neste formato: Bearer {seu_token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// 4. PIPELINE DE MIDDLEWARES HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();