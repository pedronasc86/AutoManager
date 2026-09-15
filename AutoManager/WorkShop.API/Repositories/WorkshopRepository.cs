using Microsoft.EntityFrameworkCore;
using WorkShop.API.Data;
using WorkShop.API.Models;

namespace WorkShop.API.Repositories;

public sealed class WorkshopRepository : IWorkshopRepository
{
    private readonly WorkshopContext _context;
    public WorkshopRepository(WorkshopContext context) => _context = context;

    public Task<List<Veiculo>> ObterVeiculosAsync(string? clienteId = null)
    {
        var query = _context.Veiculos.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(clienteId)) query = query.Where(v => v.ClienteId == clienteId);
        return query.OrderBy(v => v.Id).ToListAsync();
    }

    public Task<Veiculo?> ObterVeiculoAsync(int id, bool semRastreio = false)
        => (semRastreio ? _context.Veiculos.AsNoTracking() : _context.Veiculos.AsQueryable()).FirstOrDefaultAsync(v => v.Id == id);

    public Task<Veiculo?> ObterVeiculoDoClienteAsync(int id, string clienteId)
        => _context.Veiculos.FirstOrDefaultAsync(v => v.Id == id && v.ClienteId == clienteId);

    public Task<bool> MatriculaExisteAsync(string matricula, int? excluirId = null)
        => _context.Veiculos.AnyAsync(v => v.Matricula.ToUpper() == matricula && (!excluirId.HasValue || v.Id != excluirId));

    public async Task AdicionarVeiculoAsync(Veiculo veiculo) { _context.Veiculos.Add(veiculo); await GuardarAsync(); }
    public async Task RemoverVeiculoAsync(Veiculo veiculo) { _context.Veiculos.Remove(veiculo); await GuardarAsync(); }

    public Task<List<PedidoReparacao>> ObterPedidosAsync(string? estado = null, string? clienteId = null)
    {
        var query = _context.PedidosReparacao.AsQueryable();
        if (!string.IsNullOrWhiteSpace(estado)) query = query.Where(p => p.Estado == estado);
        if (!string.IsNullOrWhiteSpace(clienteId)) query = query.Where(p => p.ClienteId == clienteId);
        return query.OrderByDescending(p => p.DataSubmissao).ToListAsync();
    }

    public Task<PedidoReparacao?> ObterPedidoAsync(int id) => _context.PedidosReparacao.FindAsync(id).AsTask();
    public async Task AdicionarPedidoAsync(PedidoReparacao pedido) { _context.PedidosReparacao.Add(pedido); await GuardarAsync(); }
    public async Task AdicionarOrdemAsync(OrdemReparacao ordem) { _context.OrdensReparacao.Add(ordem); await GuardarAsync(); }

    public Task<OrdemReparacao?> ObterOrdemAsync(int id, bool incluirPecas = false)
        => incluirPecas
            ? _context.OrdensReparacao.Include(o => o.Pecas).FirstOrDefaultAsync(o => o.Id == id)
            : _context.OrdensReparacao.FindAsync(id).AsTask();

    public async Task<(List<OrdemReparacao> Itens, int TotalItens, int TotalOrdens, int TotalEmCurso, int TotalConcluidas)> ObterOrdensPaginadasAsync(int pagina, int tamanhoPagina, int? veiculoId)
    {
        var totalOrdens = await _context.OrdensReparacao.CountAsync();
        var totalEmCurso = await _context.OrdensReparacao.CountAsync(o => o.Estado == "Em Curso");
        var totalConcluidas = await _context.OrdensReparacao.CountAsync(o => o.Estado == "Concluída");
        var query = _context.OrdensReparacao.AsNoTracking();
        if (veiculoId.HasValue) query = query.Where(o => o.VeiculoId == veiculoId.Value);
        var totalItens = await query.CountAsync();
        var itens = await query.OrderBy(o => o.Id).Skip((pagina - 1) * tamanhoPagina).Take(tamanhoPagina).ToListAsync();
        return (itens, totalItens, totalOrdens, totalEmCurso, totalConcluidas);
    }

    public Task<List<OrdemReparacao>> ObterOrdensAsync(int? veiculoId = null, string? clienteId = null, bool porIdDescendente = false)
    {
        var query = _context.OrdensReparacao.AsNoTracking();
        if (veiculoId.HasValue) query = query.Where(o => o.VeiculoId == veiculoId.Value);
        if (!string.IsNullOrWhiteSpace(clienteId)) query = query.Where(o => o.ClienteId == clienteId);
        return porIdDescendente ? query.OrderByDescending(o => o.Id).ToListAsync() : query.OrderByDescending(o => o.DataEntrada).ToListAsync();
    }

    public async Task RemoverOrdemAsync(OrdemReparacao ordem) { _context.OrdensReparacao.Remove(ordem); await GuardarAsync(); }
    public Task GuardarAsync() => _context.SaveChangesAsync();
}
