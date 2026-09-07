using System.Collections.Generic;
using System.Threading.Tasks;
using WorkShop.API.DTOs;

namespace WorkShop.API.Services.Pedidos
{
    public interface IPedidoReparacaoService
    {
        Task<PedidoReparacaoResponseDto> CriarPedidoAsync(string clienteId, CriarPedidoDto dto);
        Task<IEnumerable<PedidoReparacaoResponseDto>> ObterPendentesAsync();
        Task<PedidoReparacaoResponseDto?> AtualizarEstadoAsync(int id, string novoEstado, string? observacoes);
        Task<IEnumerable<PedidoReparacaoResponseDto>> ObterPedidosDoClienteAsync(string clienteId);
        Task<IEnumerable<PedidoReparacaoResponseDto>> ObterTodosAsync();
    }
}