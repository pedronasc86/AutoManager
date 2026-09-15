using Indentity.API.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace Identity.API.Repositories;

public interface IAuthRepository
{
    Task<ApplicationUser?> ObterPorEmailAsync(string email);
    Task<ApplicationUser?> ObterPorIdAsync(string id);
    Task<ApplicationUser?> ObterUtilizadorAtualAsync(ClaimsPrincipal principal);
    Task<List<ApplicationUser>> ObterTodosAsync();
    Task<IList<ApplicationUser>> ObterPorRoleAsync(string role);
    Task<IList<string>> ObterRolesAsync(ApplicationUser user);
    Task<bool> TemRoleAsync(ApplicationUser user, string role);
    Task<bool> RoleExisteAsync(string role);
    Task<IdentityResult> CriarRoleAsync(string role);
    Task<IdentityResult> CriarAsync(ApplicationUser user, string password);
    Task<IdentityResult> AtualizarAsync(ApplicationUser user);
    Task<IdentityResult> ApagarAsync(ApplicationUser user);
    Task<IdentityResult> AdicionarRoleAsync(ApplicationUser user, string role);
    Task<bool> VerificarPasswordAsync(ApplicationUser user, string password);
    Task<string> GerarTokenReposicaoPasswordAsync(ApplicationUser user);
    Task<IdentityResult> ReporPasswordAsync(ApplicationUser user, string token, string password);
}
