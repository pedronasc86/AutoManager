using PartsCatalog.API.Models;

namespace PartsCatalog.API.Repositories
{
    public interface IPecaRepository
    {
        Task<IEnumerable<Peca>> ObterTodasAsync();
        Task<Peca?> ObterPorIdAsync(Guid id);
        Task CriarAsync(Peca peca);
        Task AtualizarAsync(Peca peca);
        Task RemoverAsync(Guid id);
        Task<bool> InativarAsync(Guid id);
        Task<bool> VerificarDisponibilidadeAsync(Guid id, int quantidade);
    }
}