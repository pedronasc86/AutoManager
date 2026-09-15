using WorkShop.API.Models;

namespace WorkShop.API.Repositories;

public interface IWorkshopRepository
{
    Task<List<Veiculo>> ObterVeiculosAsync(string? clienteId = null);
    Task<Veiculo?> ObterVeiculoAsync(int id, bool semRastreio = false);
    Task<Veiculo?> ObterVeiculoDoClienteAsync(int id, string clienteId);
    Task<bool> MatriculaExisteAsync(string matricula, int? excluirId = null);
    Task AdicionarVeiculoAsync(Veiculo veiculo);
    Task RemoverVeiculoAsync(Veiculo veiculo);
    Task<List<PedidoReparacao>> ObterPedidosAsync(string? estado = null, string? clienteId = null);
    Task<PedidoReparacao?> ObterPedidoAsync(int id);
    Task AdicionarPedidoAsync(PedidoReparacao pedido);
    Task AdicionarOrdemAsync(OrdemReparacao ordem);
    Task<OrdemReparacao?> ObterOrdemAsync(int id, bool incluirPecas = false);
    Task<(List<OrdemReparacao> Itens, int TotalItens, int TotalOrdens, int TotalEmCurso, int TotalConcluidas)> ObterOrdensPaginadasAsync(int pagina, int tamanhoPagina, int? veiculoId);
    Task<List<OrdemReparacao>> ObterOrdensAsync(int? veiculoId = null, string? clienteId = null, bool porIdDescendente = false);
    Task RemoverOrdemAsync(OrdemReparacao ordem);
    Task GuardarAsync();
}
