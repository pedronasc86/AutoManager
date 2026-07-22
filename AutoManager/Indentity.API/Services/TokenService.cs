using Indentity.API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Identity.API.Services
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _config;
        private readonly UserManager<ApplicationUser> _userManager;

        public TokenService(IConfiguration config, UserManager<ApplicationUser> userManager)
        {
            _config = config;
            _userManager = userManager;
        }
        public async Task<string> GenerateJwtTokenAsync(ApplicationUser user)
        {
            // 1. Definir as Claims Obrigatórias exigidas no RF2: sub (ID), email e roles
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),           // sub (ID do utilizador)
                new Claim(JwtRegisteredClaimNames.Email, user.Email!),     // email
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) // ID único do Token
            };

            // Adicionar as Roles (Admin, Empresa/Contabilista, Mecânico, etc.)
            var roles = await _userManager.GetRolesAsync(user);
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            // 2. Obter a Chave Secreta das configurações
            var jwtSettings = _config.GetSection("JwtSettings");
            var secretKey = jwtSettings["Secret"]
                ?? throw new InvalidOperationException("A chave JWT não está configurada no appsettings.json.");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // 3. Definir o tempo de expiração
            var expirationHours = double.Parse(jwtSettings["ExpirationInHours"] ?? "8");
            var expires = DateTime.UtcNow.AddHours(expirationHours);

            // 4. Criar o Token JWT
            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
