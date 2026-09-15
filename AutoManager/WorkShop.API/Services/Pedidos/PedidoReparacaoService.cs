using Microsoft.EntityFrameworkCore;
using WorkShop.API.Data;
using WorkShop.API.DTOs;
using WorkShop.API.Models;
using WorkShop.API.Services.Pedidos;

using WorkShop.API.Repositories;

namespace WorkShop.API.Services
{
    public class PedidoReparacaoService : IPedidoReparacaoService
    {
        private readonly IWorkshopRepository _repository;

        public PedidoReparacaoService(IWorkshopRepository repository)
        {
            _repository = repository;
        }

        public async Task<PedidoReparacaoResponseDto> CriarPedidoAsync(string clienteId, CriarPedidoDto dto)
        {
            var veiculo = await _repository.ObterVeiculoDoClienteAsync(dto.VeiculoId, clienteId);

            if (veiculo == null)
            {
                throw new UnauthorizedAccessException("O veículo especificado não pertence ao cliente autenticado ou não existe.");
            }

            var pedido = new PedidoReparacao
            {
                ClienteId = clienteId,
                VeiculoId = dto.VeiculoId,
                DescricaoProblema = dto.DescricaoProblema,
                DataSubmissao = DateTime.UtcNow,
                Estado = "Pendente"
            };

            await _repository.AdicionarPedidoAsync(pedido);

            return MapearParaDto(pedido);
        }

        public async Task<IEnumerable<PedidoReparacaoResponseDto>> ObterPendentesAsync()
        {
            var pedidos = await _repository.ObterPedidosAsync(estado: "Pendente");

            return pedidos.Select(MapearParaDto);
        }

        public async Task<PedidoReparacaoResponseDto?> AtualizarEstadoAsync(int id, string novoEstado, string? observacoes)
        {
            var pedido = await _repository.ObterPedidoAsync(id);
            if (pedido == null) return null;

            pedido.Estado = novoEstado;
            if (observacoes != null)
            {
                pedido.ObservacoesAdmin = observacoes;
            }

            await _repository.GuardarAsync();

            if (novoEstado == "Aceite")
            {
                var ordem = new OrdemReparacao
                {
                    ClienteId = pedido.ClienteId,
                    VeiculoId = pedido.VeiculoId,
                    DescricaoProblema = pedido.DescricaoProblema,
                    Estado = "Em Curso",
                    DataEntrada = DateTime.UtcNow,
                    CustoMaoDeObra = 0,
                    CustoPecas = 0
                };
                await _repository.AdicionarOrdemAsync(ordem);
            }

            return MapearParaDto(pedido);
        }

        private static PedidoReparacaoResponseDto MapearParaDto(PedidoReparacao p) => new()
        {
            Id = p.Id,
            ClienteId = p.ClienteId,
            VeiculoId = p.VeiculoId,
            DescricaoProblema = p.DescricaoProblema,
            DataSubmissao = p.DataSubmissao,
            Estado = p.Estado,
            ObservacoesAdmin = p.ObservacoesAdmin
        };

        public async Task<IEnumerable<PedidoReparacaoResponseDto>> ObterPedidosDoClienteAsync(string clienteId)
        {
            var pedidos = await _repository.ObterPedidosAsync(clienteId: clienteId);

            return pedidos.Select(MapearParaDto);
        }

        public async Task<IEnumerable<PedidoReparacaoResponseDto>> ObterTodosAsync()
        {
            var pedidos = await _repository.ObterPedidosAsync();

            return pedidos.Select(MapearParaDto);
        }
    }
}
