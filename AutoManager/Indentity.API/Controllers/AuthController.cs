using Identity.API.DTOs;
using Identity.API.Services;
using Indentity.API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace Identity.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ITokenService _tokenService;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ITokenService tokenService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _tokenService = tokenService;
        }

        // =========================================================================
        // RF1 & RF3: REGISTO DE UTILIZADORES
        // =========================================================================
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            // 1. Validar se o modelo recebido cumpre as Data Annotations (RF3)
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // 2. Verificar se o email já está registado
            var userExists = await _userManager.FindByEmailAsync(dto.Email);
            if (userExists != null)
            {
                return BadRequest(new AuthResponseDto
                {
                    IsSuccess = false,
                    Message = "Este email já se encontra registado."
                });
            }

            // 3. Criar a nova instância do utilizador
            var newUser = new ApplicationUser
            {
                UserName = dto.Email,
                Email = dto.Email,
                name = dto.FirstName.Trim()
            };

            // 4. Tentar criar o utilizador na BD (Aplica as regras de Password Strong do RF3)
            var result = await _userManager.CreateAsync(newUser, dto.Password);

            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description);
                return BadRequest(new AuthResponseDto
                {
                    IsSuccess = false,
                    Message = string.Join(" | ", errors)
                });
            }

            // 5. Garantir que a Role/Função existe na BD e associar ao utilizador
            if (!await _roleManager.RoleExistsAsync(dto.Role))
            {
                await _roleManager.CreateAsync(new IdentityRole(dto.Role));
            }

            await _userManager.AddToRoleAsync(newUser, dto.Role);

            return Ok(new AuthResponseDto
            {
                IsSuccess = true,
                Message = "Utilizador criado com sucesso!"
            });

        }
        // =========================================================================
        // RF2: LOGIN E EMISSÃO DE TOKEN JWT
        // =========================================================================
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // 1. Procurar o utilizador pelo Email
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
            {
                return Unauthorized(new AuthResponseDto
                {
                    IsSuccess = false,
                    Message = "Credenciais inválidas."
                });
            }

            // 2. Verificar se a password está correta
            var isPasswordValid = await _userManager.CheckPasswordAsync(user, dto.Password);
            if (!isPasswordValid)
            {
                return Unauthorized(new AuthResponseDto
                {
                    IsSuccess = false,
                    Message = "Credenciais inválidas."
                });
            }

            // 3. Gerar o Token JWT com as claims (sub, email, role)
            var token = await _tokenService.GenerateJwtTokenAsync(user);

            Response.Cookies.Append("jwtToken", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddHours(8)
            });

            return Ok(new AuthResponseDto
            {
                IsSuccess = true,
                Message = "Login efetuado com sucesso!",
                Expiration = DateTime.UtcNow.AddHours(8)
            });
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUser()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Unauthorized();
            }

            return Ok(new CurrentUserDto
            {
                FirstName = user.name ?? string.Empty
            });
        }

    [Authorize(Roles = "Mecanico,mecanico,Admin,admin")]
    [HttpGet("users")]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _userManager.Users
                .AsNoTracking()
                .OrderBy(user => user.name)
                .ThenBy(user => user.Email)
                .ToListAsync();

            var response = new List<UserListItemDto>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);

                response.Add(new UserListItemDto
                {
                    Id = user.Id,
                    FirstName = string.IsNullOrWhiteSpace(user.name)
                        ? user.Email ?? string.Empty
                        : user.name,
                    Email = user.Email ?? string.Empty,
                    Role = string.Join(", ", roles)
                });
            }

            return Ok(response);
        }
    }
}
