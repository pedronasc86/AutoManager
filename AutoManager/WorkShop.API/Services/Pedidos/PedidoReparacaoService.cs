using Microsoft.EntityFrameworkCore;
using WorkShop.API.Data;
using WorkShop.API.DTOs;
using WorkShop.API.Models;
using WorkShop.API.Services.Pedidos;

namespace WorkShop.API.Services
{
    public class PedidoReparacaoService : IPedidoReparacaoService
    {
        private readonly WorkshopContext _context;

        public PedidoReparacaoService(WorkshopContext context)
        {
            _context = context;
        }

        public async Task<PedidoReparacaoResponseDto> CriarPedidoAsync(string clienteId, CriarPedidoDto dto)
        {
            var veiculo = await _context.Veiculos
                .FirstOrDefaultAsync(v => v.Id == dto.VeiculoId && v.ClienteId == clienteId);

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

            _context.PedidosReparacao.Add(pedido);
            await _context.SaveChangesAsync();

            return MapearParaDto(pedido);
        }

        public async Task<IEnumerable<PedidoReparacaoResponseDto>> ObterPendentesAsync()
        {
            var pedidos = await _context.PedidosReparacao
                .Where(p => p.Estado == "Pendente")
                .OrderByDescending(p => p.DataSubmissao)
                .ToListAsync();

            return pedidos.Select(MapearParaDto);
        }

        public async Task<PedidoReparacaoResponseDto?> AtualizarEstadoAsync(int id, string novoEstado, string? observacoes)
        {
            var pedido = await _context.PedidosReparacao.FindAsync(id);
            if (pedido == null) return null;

            pedido.Estado = novoEstado;
            if (observacoes != null)
            {
                pedido.ObservacoesAdmin = observacoes;
            }

            await _context.SaveChangesAsync();

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
                _context.OrdensReparacao.Add(ordem);
                await _context.SaveChangesAsync();
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
            var pedidos = await _context.PedidosReparacao
                .Where(p => p.ClienteId == clienteId)
                .OrderByDescending(p => p.DataSubmissao)
                .ToListAsync();

            return pedidos.Select(MapearParaDto);
        }

        public async Task<IEnumerable<PedidoReparacaoResponseDto>> ObterTodosAsync()
        {
            var pedidos = await _context.PedidosReparacao
                .OrderByDescending(p => p.DataSubmissao)
                .ToListAsync();

            return pedidos.Select(MapearParaDto);
        }
    }
}