using WorkShop.API.DTOs;

namespace WorkShop.API.Services.Veiculos;

public interface IVeiculoService
{
    Task<VeiculoServiceResult> ObterTodosAsync();
    Task<VeiculoServiceResult> CriarAsync(CriarVeiculoDto dto);
    Task<VeiculoServiceResult> ObterPorIdAsync(int id);
    Task<VeiculoServiceResult> AtualizarAsync(int id, CriarVeiculoDto dto);
    Task<VeiculoServiceResult> ObterMeusAsync();
    Task<VeiculoServiceResult> EliminarAsync(int id);
}

public sealed record VeiculoServiceResult(int StatusCode, object? Data = null);
