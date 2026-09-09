using Identity.API.DTOs;

namespace Identity.API.Services;

public interface IAuthService
{
    Task<AuthServiceResult> RegistarClienteAsync(RegisterDto dto);
    Task<AuthServiceResult> LoginAsync(LoginDto dto);
    Task<AuthServiceResult> ObterUtilizadorAtualAsync();
    Task<AuthServiceResult> ObterClientesAsync();
    Task<AuthServiceResult> CriarUtilizadorPorAdminAsync(RegisterDto dto);
    Task<AuthServiceResult> ObterAdminsAsync();
    Task<AuthServiceResult> CriarAdminAsync(CriarAdminDto dto);
    Task<AuthServiceResult> AtualizarAdminAsync(string id, AtualizarAdminDto dto);
    Task<AuthServiceResult> ApagarAdminAsync(string id);
    Task<AuthServiceResult> AtualizarUtilizadorAsync(string id, AtualizarAdminDto dto);
    Task<AuthServiceResult> ApagarUtilizadorAsync(string id);
    Task<AuthServiceResult> CriarUtilizadorAsync(RegisterDto dto);
}

public sealed record AuthServiceResult(int StatusCode, object? Data = null);
