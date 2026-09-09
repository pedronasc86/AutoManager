using Microsoft.EntityFrameworkCore;
using WorkShop.API.Data;
using WorkShop.API.DTOs;
using WorkShop.API.Models;
using WorkShop.API.Services.Auth;

namespace WorkShop.API.Services.Ordens;

public class OrdensReparacaoService : IOrdensReparacaoService
{
    private readonly WorkshopContext _contexto;
    private readonly CatalogoPecasService _catalogoPecasService;
    private readonly IUserContextService _userContextService;

    public OrdensReparacaoService(WorkshopContext contexto, CatalogoPecasService catalogoPecasService, IUserContextService userContextService)
    {
        _contexto = contexto;
        _catalogoPecasService = catalogoPecasService;
        _userContextService = userContextService;
    }

    public async Task<OrdemServiceResult> ObterTodasAsync(int pagina, int tamanhoPagina, int? veiculoId)
    {
        if (veiculoId is <= 0)
            return new(400, new { mensagem = "O ID do veículo deve ser maior do que zero." });

        pagina = Math.Max(pagina, 1);
        tamanhoPagina = Math.Clamp(tamanhoPagina, 1, 20);
        var totalOrdens = await _contexto.OrdensReparacao.CountAsync();
        var totalEmCurso = await _contexto.OrdensReparacao.CountAsync(o => o.Estado == "Em Curso");
        var totalConcluidas = await _contexto.OrdensReparacao.CountAsync(o => o.Estado == "Concluída");
        var query = _contexto.OrdensReparacao.AsNoTracking();
        if (veiculoId.HasValue) query = query.Where(o => o.VeiculoId == veiculoId.Value);

        var totalItens = await query.CountAsync();
        var ordens = await query.OrderBy(o => o.Id).Skip((pagina - 1) * tamanhoPagina).Take(tamanhoPagina).ToListAsync();
        var totalPaginas = Math.Max(1, (int)Math.Ceiling(totalItens / (double)tamanhoPagina));

        return new(200, new RespostaPaginadaOrdensDto
        {
            Itens = ordens.Select(MapearParaRespostaDto).ToList(), PaginaAtual = pagina,
            TotalPaginas = totalPaginas, TotalItens = totalItens, TotalOrdens = totalOrdens,
            TotalEmCurso = totalEmCurso, TotalConcluidas = totalConcluidas
        });
    }

    public async Task<OrdemServiceResult> CriarAsync(CriarOrdemReparacaoDto dto)
    {
        var veiculo = await _contexto.Veiculos.AsNoTracking().FirstOrDefaultAsync(v => v.Id == dto.VeiculoId);
        if (veiculo is null)
            return new(400, new { mensagem = $"Veículo com ID {dto.VeiculoId} não foi encontrado." });
        if (!string.Equals(veiculo.ClienteId, dto.ClienteId, StringComparison.Ordinal))
            return new(400, new { mensagem = "O veículo indicado não pertence ao cliente indicado." });

        decimal totalCustoPecas = 0;
        var pecasDaOrdem = new List<PecaAplicadaOrdem>();
        foreach (var itemPeca in dto.Pecas ?? [])
        {
            var resultadoPeca = await _catalogoPecasService.VerificarStockEObterPrecoAsync(itemPeca.PecaId, itemPeca.Quantidade);
            if (!resultadoPeca.TemStock)
                return new(400, new { mensagem = $"Falha na validação das peças: {resultadoPeca.MensagemErro}" });

            totalCustoPecas += resultadoPeca.PrecoUnitario * itemPeca.Quantidade;
            pecasDaOrdem.Add(new PecaAplicadaOrdem
            {
                PecaId = Guid.Parse(itemPeca.PecaId), Quantidade = itemPeca.Quantidade,
                PrecoUnitario = resultadoPeca.PrecoUnitario
            });
        }

        var ordem = new OrdemReparacao
        {
            DescricaoProblema = dto.DescricaoProblema, VeiculoId = dto.VeiculoId, ClienteId = dto.ClienteId,
            DataEntrada = DateTime.UtcNow, DataConclusao = DateTime.UtcNow, Estado = "Concluída",
            CustoMaoDeObra = dto.CustoMaoDeObra, CustoPecas = totalCustoPecas, Pecas = pecasDaOrdem
        };
        _contexto.OrdensReparacao.Add(ordem);
        await _contexto.SaveChangesAsync();

        foreach (var itemPeca in dto.Pecas ?? [])
            await _catalogoPecasService.AtualizarStockAsync(itemPeca.PecaId, itemPeca.Quantidade);

        return new(201, MapearParaDetalheDto(ordem));
    }

    public async Task<OrdemServiceResult> ObterPorIdAsync(int id)
    {
        var ordem = await _contexto.OrdensReparacao.Include(o => o.Pecas).FirstOrDefaultAsync(o => o.Id == id);
        return ordem is null ? new(404) : new(200, MapearParaDetalheDto(ordem));
    }

    public async Task<OrdemServiceResult> AtualizarAsync(int id, AtualizarOrdemReparacaoDto dto)
    {
        var ordem = await _contexto.OrdensReparacao.FindAsync(id);
        if (ordem is null) return new(404, new { mensagem = $"Ordem de reparação #{id} não encontrada." });

        if (!string.IsNullOrWhiteSpace(dto.Estado))
        {
            if (dto.Estado is not ("Em Curso" or "Concluída"))
                return new(400, new { mensagem = "O estado deve ser 'Em Curso' ou 'Concluída'." });
            ordem.Estado = dto.Estado;
            ordem.DataConclusao = dto.Estado == "Concluída" ? DateTime.UtcNow : null;
        }
        if (!string.IsNullOrWhiteSpace(dto.DescricaoProblema)) ordem.DescricaoProblema = dto.DescricaoProblema.Trim();
        if (dto.CustoMaoDeObra.HasValue) ordem.CustoMaoDeObra = dto.CustoMaoDeObra.Value;
        if (dto.CustoPecas.HasValue) ordem.CustoPecas = dto.CustoPecas.Value;
        await _contexto.SaveChangesAsync();
        return new(200, MapearParaRespostaDto(ordem));
    }

    public async Task<OrdemServiceResult> ObterHistoricoPorVeiculoAsync(int veiculoId)
    {
        var ordens = await _contexto.OrdensReparacao.Where(o => o.VeiculoId == veiculoId)
            .OrderByDescending(o => o.DataEntrada).ToListAsync();
        return new(200, ordens.Select(MapearParaRespostaDto));
    }

    public async Task<OrdemServiceResult> ObterHistoricoPorClienteAsync(string clienteId)
    {
        var utilizadorAutenticadoId = _userContextService.GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(utilizadorAutenticadoId)) return new(401);
        if (!string.Equals(utilizadorAutenticadoId, clienteId, StringComparison.Ordinal)) return new(403);
        var ordens = await _contexto.OrdensReparacao.Where(o => o.ClienteId == clienteId)
            .OrderByDescending(o => o.DataEntrada).ToListAsync();
        return new(200, ordens.Select(MapearParaRespostaDto));
    }

    public async Task<OrdemServiceResult> ApagarAsync(int id)
    {
        var ordem = await _contexto.OrdensReparacao.FindAsync(id);
        if (ordem is null) return new(404);
        _contexto.OrdensReparacao.Remove(ordem);
        await _contexto.SaveChangesAsync();
        return new(204);
    }

    public async Task<OrdemServiceResult> ObterTodasSemPaginacaoAsync()
    {
        var ordens = await _contexto.OrdensReparacao.AsNoTracking().OrderByDescending(o => o.Id).ToListAsync();
        return new(200, ordens.Select(MapearParaRespostaDto));
    }

    private static RespostaOrdemReparacaoDto MapearParaRespostaDto(OrdemReparacao ordem) => new()
    {
        Id = ordem.Id, DataEntrada = ordem.DataEntrada, DataConclusao = ordem.DataConclusao,
        DescricaoProblema = ordem.DescricaoProblema, Estado = ordem.Estado,
        CustoMaoDeObra = ordem.CustoMaoDeObra, CustoPecas = ordem.CustoPecas,
        ValorTotal = ordem.ValorTotal, VeiculoId = ordem.VeiculoId, ClienteId = ordem.ClienteId
    };

    private static DetalheOrdemReparacaoDto MapearParaDetalheDto(OrdemReparacao ordem) => new()
    {
        Id = ordem.Id, DataEntrada = ordem.DataEntrada, DataConclusao = ordem.DataConclusao,
        DescricaoProblema = ordem.DescricaoProblema, Estado = ordem.Estado,
        CustoMaoDeObra = ordem.CustoMaoDeObra, CustoPecas = ordem.CustoPecas,
        ValorTotal = ordem.ValorTotal, VeiculoId = ordem.VeiculoId, ClienteId = ordem.ClienteId,
        Pecas = ordem.Pecas.Select(p => new PecaAplicadaRespostaDto
        { PecaId = p.PecaId, Quantidade = p.Quantidade, PrecoUnitario = p.PrecoUnitario }).ToList()
    };
}
