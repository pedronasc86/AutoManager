using Identity.API.DTOs;
using Identity.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Identity.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        => ConverterResposta(await _authService.RegistarClienteAsync(dto));

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
        => ConverterResposta(await _authService.LoginAsync(dto));

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser()
        => ConverterResposta(await _authService.ObterUtilizadorAtualAsync());

    [Authorize(Roles = "Admin,admin")]
    [HttpGet("users")]
    public async Task<IActionResult> GetUsers()
        => ConverterResposta(await _authService.ObterClientesAsync());

    [Authorize(Roles = "Admin")]
    [HttpPost("admin/criar-utilizador")]
    public async Task<IActionResult> CriarUtilizadorPorAdmin([FromBody] RegisterDto dto)
        => ConverterResposta(await _authService.CriarUtilizadorPorAdminAsync(dto));

    [Authorize(Roles = "Admin,admin")]
    [HttpGet("admins")]
    public async Task<IActionResult> GetAdmins()
        => ConverterResposta(await _authService.ObterAdminsAsync());

    [Authorize(Roles = "Admin,admin")]
    [HttpPost("admins")]
    public async Task<IActionResult> CreateAdmin([FromBody] CriarAdminDto dto)
        => ConverterResposta(await _authService.CriarAdminAsync(dto));

    [Authorize(Roles = "Admin,admin")]
    [HttpPut("admins/{id}")]
    public async Task<IActionResult> UpdateAdmin(string id, [FromBody] AtualizarAdminDto dto)
        => ConverterResposta(await _authService.AtualizarAdminAsync(id, dto));

    [Authorize(Roles = "Admin,admin")]
    [HttpDelete("admins/{id}")]
    public async Task<IActionResult> DeleteAdmin(string id)
        => ConverterResposta(await _authService.ApagarAdminAsync(id));

    [Authorize(Roles = "Admin,admin")]
    [HttpPut("users/{id}")]
    public async Task<IActionResult> UpdateUser(string id, [FromBody] AtualizarAdminDto dto)
        => ConverterResposta(await _authService.AtualizarUtilizadorAsync(id, dto));

    [Authorize(Roles = "Admin,admin")]
    [HttpDelete("users/{id}")]
    public async Task<IActionResult> DeleteUser(string id)
        => ConverterResposta(await _authService.ApagarUtilizadorAsync(id));

    [Authorize(Roles = "Admin,admin")]
    [HttpPost("users")]
    public async Task<IActionResult> CreateUser([FromBody] RegisterDto dto)
        => ConverterResposta(await _authService.CriarUtilizadorAsync(dto));

    private IActionResult ConverterResposta(AuthServiceResult resultado)
        => StatusCode(resultado.StatusCode, resultado.Data);
}
