using Identity.API.DTOs;
using Identity.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Identity.API.Controllers;

/// <summary>Gere o registo, autenticação e administração das contas de utilizador.</summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>Regista uma nova conta de cliente.</summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        => ConverterResposta(await _authService.RegistarClienteAsync(dto));

    /// <summary>Autentica um utilizador e devolve um token de acesso.</summary>
    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
        => ConverterResposta(await _authService.LoginAsync(dto));

    /// <summary>Obtém os dados do utilizador autenticado.</summary>
    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType(typeof(CurrentUserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCurrentUser()
        => ConverterResposta(await _authService.ObterUtilizadorAtualAsync());

    /// <summary>Lista todas as contas com o perfil de cliente.</summary>
    [Authorize(Roles = "Admin,admin")]
    [HttpGet("users")]
    [ProducesResponseType(typeof(IEnumerable<UserListItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetUsers()
        => ConverterResposta(await _authService.ObterClientesAsync());

    /// <summary>Cria uma conta com um dos perfis permitidos para um administrador.</summary>
    [Authorize(Roles = "Admin")]
    [HttpPost("admin/criar-utilizador")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CriarUtilizadorPorAdmin([FromBody] RegisterDto dto)
        => ConverterResposta(await _authService.CriarUtilizadorPorAdminAsync(dto));

    /// <summary>Lista as contas que têm o perfil de administrador.</summary>
    [Authorize(Roles = "Admin,admin")]
    [HttpGet("admins")]
    [ProducesResponseType(typeof(IEnumerable<UserListItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAdmins()
        => ConverterResposta(await _authService.ObterAdminsAsync());

    /// <summary>Cria uma nova conta de administrador.</summary>
    [Authorize(Roles = "Admin,admin")]
    [HttpPost("admins")]
    [ProducesResponseType(typeof(UserListItemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateAdmin([FromBody] CriarAdminDto dto)
        => ConverterResposta(await _authService.CriarAdminAsync(dto));

    /// <summary>Atualiza os dados de uma conta de administrador.</summary>
    [Authorize(Roles = "Admin,admin")]
    [HttpPut("admins/{id}")]
    [ProducesResponseType(typeof(UserListItemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateAdmin(string id, [FromBody] AtualizarAdminDto dto)
        => ConverterResposta(await _authService.AtualizarAdminAsync(id, dto));

    /// <summary>Elimina uma conta de administrador, exceto a própria ou o último administrador.</summary>
    [Authorize(Roles = "Admin,admin")]
    [HttpDelete("admins/{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAdmin(string id)
        => ConverterResposta(await _authService.ApagarAdminAsync(id));

    /// <summary>Atualiza os dados de uma conta de utilizador.</summary>
    [Authorize(Roles = "Admin,admin")]
    [HttpPut("users/{id}")]
    [ProducesResponseType(typeof(UserListItemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateUser(string id, [FromBody] AtualizarAdminDto dto)
        => ConverterResposta(await _authService.AtualizarUtilizadorAsync(id, dto));

    /// <summary>Elimina uma conta de utilizador.</summary>
    [Authorize(Roles = "Admin,admin")]
    [HttpDelete("users/{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteUser(string id)
        => ConverterResposta(await _authService.ApagarUtilizadorAsync(id));

    /// <summary>Cria uma conta de utilizador e atribui-lhe o perfil indicado.</summary>
    [Authorize(Roles = "Admin,admin")]
    [HttpPost("users")]
    [ProducesResponseType(typeof(UserListItemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateUser([FromBody] RegisterDto dto)
        => ConverterResposta(await _authService.CriarUtilizadorAsync(dto));

    private IActionResult ConverterResposta(AuthServiceResult resultado)
        => StatusCode(resultado.StatusCode, resultado.Data);
}
