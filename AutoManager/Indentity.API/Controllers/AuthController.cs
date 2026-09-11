using Identity.API.DTOs;
using Identity.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Identity.API.Controllers
{
    /// <summary>Gere o registo, autenticação e administração das contas de utilizador.</summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ITokenService _tokenService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

        /// <summary>Regista uma nova conta de cliente.</summary>
        /// <param name="dto">Dados de registo da conta.</param>
        [HttpPost("register")]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userExists = await _userManager.FindByEmailAsync(dto.Email);

            if (userExists != null)
            {
                return BadRequest(new AuthResponseDto
                {
                    IsSuccess = false,
                    Message = "Este email já se encontra registado."
                });
            }

            var newUser = new ApplicationUser
            {
                UserName = dto.Email,
                Email = dto.Email,
                name = dto.FirstName.Trim()
            };

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

            const string roleCliente = "Cliente";

            if (!await _roleManager.RoleExistsAsync(roleCliente))
            {
                await _roleManager.CreateAsync(new IdentityRole(roleCliente));
            }

            await _userManager.AddToRoleAsync(newUser, roleCliente);

            return Ok(new AuthResponseDto
            {
                IsSuccess = true,
                Message = "Utilizador criado com sucesso!"
            });
        }

        /// <summary>Autentica um utilizador e devolve um token de acesso.</summary>
        /// <param name="dto">Credenciais de autenticação.</param>
        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userManager.FindByEmailAsync(dto.Email);

            if (user == null)
            {
                return Unauthorized(new AuthResponseDto
                {
                    IsSuccess = false,
                    Message = "Credenciais inválidas."
                });
            }

            var isPasswordValid =
                await _userManager.CheckPasswordAsync(user, dto.Password);

            if (!isPasswordValid)
            {
                return Unauthorized(new AuthResponseDto
                {
                    IsSuccess = false,
                    Message = "Credenciais inválidas."
                });
            }

            var token = await _tokenService.GenerateJwtTokenAsync(user);

            Response.Cookies.Append("jwtToken", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddHours(8)
            });

            return Ok(new
            {
                isSuccess = true,
                message = "Login efetuado com sucesso!",
                token = token,
                expiration = DateTime.UtcNow.AddHours(8)
            });
        }

        /// <summary>Obtém os dados do utilizador autenticado.</summary>
        [Authorize]
        [HttpGet("me")]
        [ProducesResponseType(typeof(CurrentUserDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetCurrentUser()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Unauthorized();
            }

            var roles = await _userManager.GetRolesAsync(user);

            return Ok(new CurrentUserDto
            {
                FirstName = user.name ?? string.Empty,
                Role = roles.FirstOrDefault() ?? string.Empty
            });
        }

        /// <summary>Lista todas as contas com o perfil de cliente.</summary>
        [Authorize(Roles = "Admin,admin")]
        [HttpGet("users")]
        [ProducesResponseType(typeof(IEnumerable<UserListItemDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _userManager.Users.ToListAsync();
            var userList = new List<UserListItemDto>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);

                // Filtra estritamente para incluir apenas a role "Cliente"
                if (roles.Contains("Cliente", StringComparer.OrdinalIgnoreCase))
                {
                    userList.Add(new UserListItemDto
                    {
                        Id = user.Id,
                        FirstName = user.name,
                        Email = user.Email,
                        Role = roles.FirstOrDefault() ?? "Cliente"
                    });
                }
            }

            return Ok(userList);
        }

        /// <summary>Cria uma conta com um dos perfis permitidos para um administrador.</summary>
        /// <param name="dto">Dados da conta e perfil a criar.</param>
        [Authorize(Roles = "Admin")]
        [HttpPost("admin/criar-utilizador")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> CriarUtilizadorPorAdmin(
            [FromBody] RegisterDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            string[] rolesPermitidas = { "Cliente", "Mecanico", "Admin" };

            var role = rolesPermitidas.FirstOrDefault(r =>
                r.Equals(dto.Role, StringComparison.OrdinalIgnoreCase));

            if (role == null)
            {
                return BadRequest(new
                {
                    message = "Role inválida. Escolha Cliente, Mecanico ou Admin."
                });
            }

            var userExists = await _userManager.FindByEmailAsync(dto.Email);

            if (userExists != null)
            {
                return BadRequest(new
                {
                    message = "Este email já se encontra registado."
                });
            }

            var newUser = new ApplicationUser
            {
                UserName = dto.Email,
                Email = dto.Email,
                name = dto.FirstName.Trim()
            };

            var result = await _userManager.CreateAsync(newUser, dto.Password);

            if (!result.Succeeded)
            {
                return BadRequest(new
                {
                    message = string.Join(
                        " | ",
                        result.Errors.Select(e => e.Description)
                    )
                });
            }

            await _userManager.AddToRoleAsync(newUser, role);

            return Ok(new
            {
                isSuccess = true,
                message = $"Utilizador criado com a role {role}."
            });
        }

        /// <summary>Lista as contas que têm o perfil de administrador.</summary>
        [Authorize(Roles = "Admin,admin")]
        [HttpGet("admins")]
        [ProducesResponseType(typeof(IEnumerable<UserListItemDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetAdmins()
        {
            var admins = await _userManager.GetUsersInRoleAsync("Admin");

            var response = admins
                .OrderBy(admin => admin.name)
                .ThenBy(admin => admin.Email)
                .Select(admin => new UserListItemDto
                {
                    Id = admin.Id,
                    FirstName = string.IsNullOrWhiteSpace(admin.name)
                        ? admin.Email ?? string.Empty
                        : admin.name,
                    Email = admin.Email ?? string.Empty,
                    Role = "Admin"
                })
                .ToList();

            return Ok(response);
        }

        /// <summary>Cria uma nova conta de administrador.</summary>
        /// <param name="dto">Dados da conta de administrador a criar.</param>
        [Authorize(Roles = "Admin,admin")]
        [HttpPost("admins")]
        [ProducesResponseType(typeof(UserListItemDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> CreateAdmin([FromBody] CriarAdminDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var email = dto.Email.Trim();
            var userExists = await _userManager.FindByEmailAsync(email);

            if (userExists != null)
            {
                return BadRequest(new
                {
                    message = "Já existe uma conta com este e-mail."
                });
            }

            if (!await _roleManager.RoleExistsAsync("Admin"))
            {
                var roleResult = await _roleManager.CreateAsync(
                    new IdentityRole("Admin")
                );

                if (!roleResult.Succeeded)
                {
                    return BadRequest(new
                    {
                        message = "Não foi possível criar o perfil Admin."
                    });
                }
            }

            var admin = new ApplicationUser
            {
                UserName = email,
                Email = email,
                name = dto.FirstName.Trim()
            };

            var createResult = await _userManager.CreateAsync(
                admin,
                dto.Password
            );

            if (!createResult.Succeeded)
            {
                return BadRequest(new
                {
                    message = string.Join(
                        " | ",
                        createResult.Errors.Select(error => error.Description)
                    )
                });
            }

            var addRoleResult = await _userManager.AddToRoleAsync(
                admin,
                "Admin"
            );

            if (!addRoleResult.Succeeded)
            {
                await _userManager.DeleteAsync(admin);

                return BadRequest(new
                {
                    message = "Não foi possível atribuir o perfil Admin."
                });
            }

            return Ok(new UserListItemDto
            {
                Id = admin.Id,
                FirstName = admin.name,
                Email = admin.Email,
                Role = "Admin"
            });
        }

        /// <summary>Atualiza os dados de uma conta de administrador.</summary>
        /// <param name="id">Identificador da conta de administrador.</param>
        /// <param name="dto">Novos dados da conta.</param>
        [Authorize(Roles = "Admin,admin")]
        [HttpPut("admins/{id}")]
        [ProducesResponseType(typeof(UserListItemDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateAdmin(
            string id,
            [FromBody] AtualizarAdminDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var admin = await _userManager.FindByIdAsync(id);

            if (admin == null ||
                !await _userManager.IsInRoleAsync(admin, "Admin"))
            {
                return NotFound(new
                {
                    message = "Administrador não encontrado."
                });
            }

            var email = dto.Email.Trim();
            var userWithEmail = await _userManager.FindByEmailAsync(email);

            if (userWithEmail != null && userWithEmail.Id != admin.Id)
            {
                return BadRequest(new
                {
                    message = "Já existe uma conta com este e-mail."
                });
            }

            admin.name = dto.FirstName.Trim();
            admin.Email = email;
            admin.UserName = email;

            var updateResult = await _userManager.UpdateAsync(admin);

            if (!updateResult.Succeeded)
            {
                return BadRequest(new
                {
                    message = string.Join(
                        " | ",
                        updateResult.Errors.Select(error => error.Description)
                    )
                });
            }

            if (!string.IsNullOrWhiteSpace(dto.Password))
            {
                var resetToken =
                    await _userManager.GeneratePasswordResetTokenAsync(admin);

                var passwordResult = await _userManager.ResetPasswordAsync(
                    admin,
                    resetToken,
                    dto.Password
                );

                if (!passwordResult.Succeeded)
                {
                    return BadRequest(new
                    {
                        message = string.Join(
                            " | ",
                            passwordResult.Errors.Select(
                                error => error.Description
                            )
                        )
                    });
                }
            }

            return Ok(new UserListItemDto
            {
                Id = admin.Id,
                FirstName = admin.name,
                Email = admin.Email,
                Role = "Admin"
            });
        }

        /// <summary>Elimina uma conta de administrador, exceto a própria ou o último administrador.</summary>
        /// <param name="id">Identificador da conta de administrador.</param>
        [Authorize(Roles = "Admin,admin")]
        [HttpDelete("admins/{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteAdmin(string id)
        {
            var currentUserId = _userManager.GetUserId(User);

            if (id == currentUserId)
            {
                return BadRequest(new
                {
                    message = "Não podes eliminar a tua própria conta."
                });
            }

            var admin = await _userManager.FindByIdAsync(id);

            if (admin == null ||
                !await _userManager.IsInRoleAsync(admin, "Admin"))
            {
                return NotFound(new
                {
                    message = "Administrador não encontrado."
                });
            }

            var admins = await _userManager.GetUsersInRoleAsync("Admin");

            if (admins.Count <= 1)
            {
                return BadRequest(new
                {
                    message = "Não é possível eliminar o último administrador."
                });
            }

            var deleteResult = await _userManager.DeleteAsync(admin);

            if (!deleteResult.Succeeded)
            {
                return BadRequest(new
                {
                    message = string.Join(
                        " | ",
                        deleteResult.Errors.Select(error => error.Description)
                    )
                });
            }

            return NoContent();
        }
        /// <summary>Atualiza os dados de uma conta de utilizador.</summary>
        /// <param name="id">Identificador da conta de utilizador.</param>
        /// <param name="dto">Novos dados da conta.</param>
        [Authorize(Roles = "Admin,admin")]
        [HttpPut("users/{id}")]
        [ProducesResponseType(typeof(UserListItemDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] AtualizarAdminDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound(new { message = "Utilizador não encontrado." });

            var email = dto.Email.Trim();
            var userWithEmail = await _userManager.FindByEmailAsync(email);
            if (userWithEmail != null && userWithEmail.Id != user.Id)
            {
                return BadRequest(new { message = "Já existe uma conta com este e-mail." });
            }

            user.name = dto.FirstName.Trim();
            user.Email = email;
            user.UserName = email;

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                return BadRequest(new { message = string.Join(" | ", updateResult.Errors.Select(e => e.Description)) });
            }

            if (!string.IsNullOrWhiteSpace(dto.Password))
            {
                var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
                await _userManager.ResetPasswordAsync(user, resetToken, dto.Password);
            }

            return Ok(new UserListItemDto
            {
                Id = user.Id,
                FirstName = user.name,
                Email = user.Email,
                Role = (await _userManager.GetRolesAsync(user)).FirstOrDefault() ?? "Cliente"
            });
        }

        /// <summary>Elimina uma conta de utilizador.</summary>
        /// <param name="id">Identificador da conta de utilizador.</param>
        [Authorize(Roles = "Admin,admin")]
        [HttpDelete("users/{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound(new { message = "Utilizador não encontrado." });

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                return BadRequest(new { message = string.Join(" | ", result.Errors.Select(e => e.Description)) });
            }

            return NoContent();
        }
        /// <summary>Cria uma conta de utilizador e atribui-lhe o perfil indicado.</summary>
        /// <param name="dto">Dados da conta a criar.</param>
        [Authorize(Roles = "Admin,admin")]
        [HttpPost("users")]
        [ProducesResponseType(typeof(UserListItemDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateUser([FromBody] RegisterDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existingUser = await _userManager.FindByEmailAsync(dto.Email);
            if (existingUser != null)
            {
                return BadRequest(new { message = "Já existe um utilizador com este e-mail." });
            }

            var user = new ApplicationUser
            {
                UserName = dto.Email,
                Email = dto.Email,
                name = dto.FirstName
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join(" | ", result.Errors.Select(e => e.Description));
                return BadRequest(new { message = errors });
            }

            // Atribuir a role recebida no DTO ou definir "Cliente" por defeito
            var roleToAssign = string.IsNullOrWhiteSpace(dto.Role) ? "Cliente" : dto.Role;
            if (!await _roleManager.RoleExistsAsync(roleToAssign))
            {
                await _roleManager.CreateAsync(new IdentityRole(roleToAssign));
            }

            await _userManager.AddToRoleAsync(user, roleToAssign);

            return Ok(new UserListItemDto
            {
                Id = user.Id,
                FirstName = user.name,
                Email = user.Email,
                Role = roleToAssign
            });
        }
    }
}
