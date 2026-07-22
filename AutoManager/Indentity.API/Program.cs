using Identity.API.Services;
using Indentity.API.Data;
using Indentity.API.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// =========================================================================
// 1. REGISTO DOS CONTROLLERS (Essencial para o Swagger encontrar os endpoints!)
// =========================================================================
builder.Services.AddControllers();

// =========================================================================
// 2. CONFIGURAÇÃO DA BASE DE DADOS (EF Core)
// =========================================================================
builder.Services.AddDbContext<AutoManagerDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// =========================================================================
// 3. ASP.NET CORE IDENTITY & REQUISITO RF3 (Password Strong)
// =========================================================================
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // RF3: Mínimo 8 caracteres, números e símbolos[cite: 1]
    options.Password.RequiredLength = 8;
    options.Password.RequireDigit = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<AutoManagerDbContext>()
.AddDefaultTokenProviders();

// =========================================================================
// 4. INJEÇÃO DE DEPENDÊNCIAS DOS TEUS SERVIÇOS
// =========================================================================
builder.Services.AddScoped<ITokenService, TokenService>();

// =========================================================================
// 5. CONFIGURAÇÃO DO JWT BEARER (AUTENTICAÇÃO)
// =========================================================================
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["Secret"]
    ?? throw new InvalidOperationException("A chave 'Secret' do JWT não está definida no appsettings.json.");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
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
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
    };
});

builder.Services.AddAuthorization();

// =========================================================================
// 6. CONFIGURAÇÃO DO SWAGGER
// =========================================================================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Identity.API", Version = "v1" });

    // Permite introduzir o Token JWT para testar rotas protegidas
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

// =========================================================================
// 7. PIPELINE DE MIDDLEWARES HTTP
// =========================================================================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// A ordem destes middlewares é obrigatória:
app.UseAuthentication(); // 1º Valida o Token
app.UseAuthorization();  // 2º Valida permissões/roles

// Mapeia os endpoints dos Controllers (necessário para os endpoints aparecerem no Swagger)
app.MapControllers();

app.Run();