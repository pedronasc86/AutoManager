using PartsCatalog.API.Models;

using PartsCatalog.API.DTOs;

namespace PartsCatalog.API.Services
{
    public interface IPecaService
    {
        IQueryable<Peca> GetPartsQuery();
        Task<PecaServiceResult> ObterPorIdAsync(Guid id);
        Task<PecaServiceResult> CriarAsync(CriarPecaRequest request);
        Task<PecaServiceResult> AtualizarAsync(Guid id, AtualizarPecaRequest request);
        Task<PecaServiceResult> RemoverAsync(Guid id);
        Task<PecaServiceResult> InativarAsync(Guid id);
        Task<PecaServiceResult> AtivarAsync(Guid id);
        Task<PecaServiceResult> VerificarDisponibilidadeAsync(Guid id, int quantidade);
        Task<PecaServiceResult> ObterTodasAdminAsync();
    }

    public sealed record PecaServiceResult(int StatusCode, object? Data = null);
}
