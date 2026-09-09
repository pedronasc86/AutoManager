using Identity.API.DTOs;
using Indentity.API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Identity.API.Services;

public class AuthService : IAuthService
{
    private static readonly string[] RolesPermitidas = ["Cliente", "Mecanico", "Admin"];
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ITokenService _tokenService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuthService(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager,
        ITokenService tokenService, IHttpContextAccessor httpContextAccessor)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _tokenService = tokenService;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<AuthServiceResult> RegistarClienteAsync(RegisterDto dto)
    {
        if (await _userManager.FindByEmailAsync(dto.Email) is not null)
            return Falha("Este email já se encontra registado.");

        var user = CriarModeloUtilizador(dto.Email, dto.FirstName);
        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded) return Falha(result.Errors);

        var roleResult = await GarantirEAdicionarRoleAsync(user, "Cliente");
        if (!roleResult.Succeeded)
        {
            await _userManager.DeleteAsync(user);
            return Falha(roleResult.Errors);
        }

        return new(200, new AuthResponseDto { IsSuccess = true, Message = "Utilizador criado com sucesso!" });
    }

    public async Task<AuthServiceResult> LoginAsync(LoginDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user is null || !await _userManager.CheckPasswordAsync(user, dto.Password))
            return new(401, new AuthResponseDto { IsSuccess = false, Message = "Credenciais inválidas." });

        var token = await _tokenService.GenerateJwtTokenAsync(user);
        var expiration = DateTime.UtcNow.AddHours(8);
        _httpContextAccessor.HttpContext?.Response.Cookies.Append("jwtToken", token, new CookieOptions
        {
            HttpOnly = true, Secure = true, SameSite = SameSiteMode.Strict, Expires = new DateTimeOffset(expiration)
        });

        return new(200, new LoginResponseDto(true, "Login efetuado com sucesso!", token, expiration));
    }

    public async Task<AuthServiceResult> ObterUtilizadorAtualAsync()
    {
        var user = await ObterUtilizadorDoPedidoAsync();
        if (user is null) return new(401);
        var roles = await _userManager.GetRolesAsync(user);
        return new(200, new CurrentUserDto { FirstName = user.name ?? string.Empty, Role = roles.FirstOrDefault() ?? string.Empty });
    }

    public async Task<AuthServiceResult> ObterClientesAsync()
    {
        var users = await _userManager.Users.ToListAsync();
        var lista = new List<UserListItemDto>();
        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Contains("Cliente", StringComparer.OrdinalIgnoreCase)) lista.Add(MapearUtilizador(user, roles.FirstOrDefault() ?? "Cliente"));
        }
        return new(200, lista);
    }

    public async Task<AuthServiceResult> CriarUtilizadorPorAdminAsync(RegisterDto dto)
    {
        var role = ObterRoleValida(dto.Role);
        if (role is null) return new(400, new { message = "Role inválida. Escolha Cliente, Mecanico ou Admin." });
        return await CriarUtilizadorComRoleAsync(dto, role, "Este email já se encontra registado.", "Utilizador criado com a role");
    }

    public async Task<AuthServiceResult> ObterAdminsAsync()
    {
        var admins = await _userManager.GetUsersInRoleAsync("Admin");
        var resposta = admins.OrderBy(admin => admin.name).ThenBy(admin => admin.Email)
            .Select(admin => MapearUtilizador(admin, "Admin")).ToList();
        return new(200, resposta);
    }

    public async Task<AuthServiceResult> CriarAdminAsync(CriarAdminDto dto)
    {
        var email = dto.Email.Trim();
        if (await _userManager.FindByEmailAsync(email) is not null)
            return new(400, new { message = "Já existe uma conta com este e-mail." });

        var admin = CriarModeloUtilizador(email, dto.FirstName);
        var createResult = await _userManager.CreateAsync(admin, dto.Password);
        if (!createResult.Succeeded) return Falha(createResult.Errors);

        var roleResult = await GarantirEAdicionarRoleAsync(admin, "Admin");
        if (!roleResult.Succeeded)
        {
            await _userManager.DeleteAsync(admin);
            return Falha(roleResult.Errors);
        }
        return new(200, MapearUtilizador(admin, "Admin"));
    }

    public async Task<AuthServiceResult> AtualizarAdminAsync(string id, AtualizarAdminDto dto)
    {
        var admin = await _userManager.FindByIdAsync(id);
        if (admin is null || !await _userManager.IsInRoleAsync(admin, "Admin"))
            return new(404, new { message = "Administrador não encontrado." });
        return await AtualizarDadosUtilizadorAsync(admin, dto, "Admin");
    }

    public async Task<AuthServiceResult> ApagarAdminAsync(string id)
    {
        var atual = await ObterUtilizadorDoPedidoAsync();
        if (atual?.Id == id) return new(400, new { message = "Não podes eliminar a tua própria conta." });

        var admin = await _userManager.FindByIdAsync(id);
        if (admin is null || !await _userManager.IsInRoleAsync(admin, "Admin"))
            return new(404, new { message = "Administrador não encontrado." });

        if ((await _userManager.GetUsersInRoleAsync("Admin")).Count <= 1)
            return new(400, new { message = "Não é possível eliminar o último administrador." });

        var result = await _userManager.DeleteAsync(admin);
        return result.Succeeded ? new(204) : Falha(result.Errors);
    }

    public async Task<AuthServiceResult> AtualizarUtilizadorAsync(string id, AtualizarAdminDto dto)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null) return new(404, new { message = "Utilizador não encontrado." });
        var roles = await _userManager.GetRolesAsync(user);
        return await AtualizarDadosUtilizadorAsync(user, dto, roles.FirstOrDefault() ?? "Cliente");
    }

    public async Task<AuthServiceResult> ApagarUtilizadorAsync(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null) return new(404, new { message = "Utilizador não encontrado." });
        var result = await _userManager.DeleteAsync(user);
        return result.Succeeded ? new(204) : Falha(result.Errors);
    }

    public async Task<AuthServiceResult> CriarUtilizadorAsync(RegisterDto dto)
    {
        var role = string.IsNullOrWhiteSpace(dto.Role) ? "Cliente" : ObterRoleValida(dto.Role);
        if (role is null) return new(400, new { message = "Role inválida. Escolha Cliente, Mecanico ou Admin." });
        return await CriarUtilizadorComRoleAsync(dto, role, "Já existe um utilizador com este e-mail.", null);
    }

    private async Task<AuthServiceResult> CriarUtilizadorComRoleAsync(RegisterDto dto, string role, string erroEmail, string? mensagemSucesso)
    {
        var email = dto.Email.Trim();
        if (await _userManager.FindByEmailAsync(email) is not null) return new(400, new { message = erroEmail });
        var user = CriarModeloUtilizador(email, dto.FirstName);
        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded) return Falha(result.Errors);

        var roleResult = await GarantirEAdicionarRoleAsync(user, role);
        if (!roleResult.Succeeded)
        {
            await _userManager.DeleteAsync(user);
            return Falha(roleResult.Errors);
        }

        if (mensagemSucesso is not null) return new(200, new { isSuccess = true, message = $"{mensagemSucesso} {role}." });
        return new(200, MapearUtilizador(user, role));
    }

    private async Task<AuthServiceResult> AtualizarDadosUtilizadorAsync(ApplicationUser user, AtualizarAdminDto dto, string role)
    {
        var email = dto.Email.Trim();
        var comMesmoEmail = await _userManager.FindByEmailAsync(email);
        if (comMesmoEmail is not null && comMesmoEmail.Id != user.Id) return new(400, new { message = "Já existe uma conta com este e-mail." });

        user.name = dto.FirstName.Trim();
        user.Email = email;
        user.UserName = email;
        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded) return Falha(updateResult.Errors);

        if (!string.IsNullOrWhiteSpace(dto.Password))
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var passwordResult = await _userManager.ResetPasswordAsync(user, token, dto.Password);
            if (!passwordResult.Succeeded) return Falha(passwordResult.Errors);
        }
        return new(200, MapearUtilizador(user, role));
    }

    private async Task<IdentityResult> GarantirEAdicionarRoleAsync(ApplicationUser user, string role)
    {
        if (!await _roleManager.RoleExistsAsync(role))
        {
            var criarRole = await _roleManager.CreateAsync(new IdentityRole(role));
            if (!criarRole.Succeeded) return criarRole;
        }
        return await _userManager.AddToRoleAsync(user, role);
    }

    private Task<ApplicationUser?> ObterUtilizadorDoPedidoAsync()
        => _httpContextAccessor.HttpContext is null
            ? Task.FromResult<ApplicationUser?>(null)
            : _userManager.GetUserAsync(_httpContextAccessor.HttpContext.User);

    private static ApplicationUser CriarModeloUtilizador(string email, string nome) => new()
    {
        UserName = email.Trim(), Email = email.Trim(), name = nome.Trim()
    };

    private static string? ObterRoleValida(string? role) => RolesPermitidas.FirstOrDefault(r => r.Equals(role, StringComparison.OrdinalIgnoreCase));
    private static UserListItemDto MapearUtilizador(ApplicationUser user, string role) => new()
    {
        Id = user.Id, FirstName = string.IsNullOrWhiteSpace(user.name) ? user.Email ?? string.Empty : user.name,
        Email = user.Email ?? string.Empty, Role = role
    };
    private static AuthServiceResult Falha(IEnumerable<IdentityError> errors) => Falha(string.Join(" | ", errors.Select(e => e.Description)));
    private static AuthServiceResult Falha(string mensagem) => new(400, new { isSuccess = false, message = mensagem });

    private sealed record LoginResponseDto(bool IsSuccess, string Message, string Token, DateTime Expiration);
}
