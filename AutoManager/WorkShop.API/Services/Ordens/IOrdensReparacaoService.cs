using WorkShop.API.DTOs;

namespace WorkShop.API.Services.Ordens;

public interface IOrdensReparacaoService
{
    Task<OrdemServiceResult> ObterTodasAsync(int pagina, int tamanhoPagina, int? veiculoId);
    Task<OrdemServiceResult> CriarAsync(CriarOrdemReparacaoDto dto);
    Task<OrdemServiceResult> ObterPorIdAsync(int id);
    Task<OrdemServiceResult> AtualizarAsync(int id, AtualizarOrdemReparacaoDto dto);
    Task<OrdemServiceResult> ObterHistoricoPorVeiculoAsync(int veiculoId);
    Task<OrdemServiceResult> ObterHistoricoPorClienteAsync(string clienteId);
    Task<OrdemServiceResult> ApagarAsync(int id);
    Task<OrdemServiceResult> ObterTodasSemPaginacaoAsync();
}

public sealed record OrdemServiceResult(int StatusCode, object? Data = null);
