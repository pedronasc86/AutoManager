using Indentity.API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Identity.API.Repositories;

public sealed class AuthRepository : IAuthRepository
{
    private readonly UserManager<ApplicationUser> _users;
    private readonly RoleManager<IdentityRole> _roles;

    public AuthRepository(UserManager<ApplicationUser> users, RoleManager<IdentityRole> roles) { _users = users; _roles = roles; }
    public Task<ApplicationUser?> ObterPorEmailAsync(string email) => _users.FindByEmailAsync(email);
    public Task<ApplicationUser?> ObterPorIdAsync(string id) => _users.FindByIdAsync(id);
    public Task<ApplicationUser?> ObterUtilizadorAtualAsync(ClaimsPrincipal principal) => _users.GetUserAsync(principal);
    public Task<List<ApplicationUser>> ObterTodosAsync() => _users.Users.ToListAsync();
    public Task<IList<ApplicationUser>> ObterPorRoleAsync(string role) => _users.GetUsersInRoleAsync(role);
    public Task<IList<string>> ObterRolesAsync(ApplicationUser user) => _users.GetRolesAsync(user);
    public Task<bool> TemRoleAsync(ApplicationUser user, string role) => _users.IsInRoleAsync(user, role);
    public Task<bool> RoleExisteAsync(string role) => _roles.RoleExistsAsync(role);
    public Task<IdentityResult> CriarRoleAsync(string role) => _roles.CreateAsync(new IdentityRole(role));
    public Task<IdentityResult> CriarAsync(ApplicationUser user, string password) => _users.CreateAsync(user, password);
    public Task<IdentityResult> AtualizarAsync(ApplicationUser user) => _users.UpdateAsync(user);
    public Task<IdentityResult> ApagarAsync(ApplicationUser user) => _users.DeleteAsync(user);
    public Task<IdentityResult> AdicionarRoleAsync(ApplicationUser user, string role) => _users.AddToRoleAsync(user, role);
    public Task<bool> VerificarPasswordAsync(ApplicationUser user, string password) => _users.CheckPasswordAsync(user, password);
    public Task<string> GerarTokenReposicaoPasswordAsync(ApplicationUser user) => _users.GeneratePasswordResetTokenAsync(user);
    public Task<IdentityResult> ReporPasswordAsync(ApplicationUser user, string token, string password) => _users.ResetPasswordAsync(user, token, password);
}
