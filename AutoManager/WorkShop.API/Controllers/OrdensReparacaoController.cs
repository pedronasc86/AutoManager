using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorkShop.API.Data;
using WorkShop.API.DTOs;
using WorkShop.API.Models;
using WorkShop.API.Services;

namespace WorkShop.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // RF8 / RF9: Exige que o utilizador esteja autenticado
    public class OrdensReparacaoController : ControllerBase
    {
        private readonly WorkshopContext _contexto;
        private readonly CatalogoPecasService _catalogoPecasService;

        public OrdensReparacaoController(WorkshopContext contexto, CatalogoPecasService catalogoPecasService)
        {
            _contexto = contexto;
            _catalogoPecasService = catalogoPecasService;
        }

        // RF8: POST /api/OrdensReparacao
        [HttpPost]
        public async Task<IActionResult> CriarOrdem([FromBody] CriarOrdemReparacaoDto dto)
        {
            // 1. Validar se o veículo existe
            var veiculoExiste = await _contexto.Veiculos.AnyAsync(v => v.Id == dto.VeiculoId);
            if (!veiculoExiste)
            {
                return BadRequest($"Veículo com ID {dto.VeiculoId} não foi encontrado.");
            }

            decimal totalCustoPecas = 0;

            // 2. Consulta a PartsCatalog.API (via HttpClient) para verificar stock e preços
            if (dto.Pecas != null && dto.Pecas.Count > 0)
            {
                foreach (var itemPeca in dto.Pecas)
                {
                    var (temStock, precoUnitario, mensagemErro) = await _catalogoPecasService.VerificarStockEObterPrecoAsync(itemPeca.PecaId, itemPeca.Quantidade);

                    if (!temStock)
                    {
                        return BadRequest($"Falha na validação das peças: {mensagemErro}");
                    }

                    totalCustoPecas += (precoUnitario * itemPeca.Quantidade);
                }
            }

            // 3. Guarda a Ordem de Reparação na BD com estado "Em Curso" e calcula o total
            var ordem = new OrdemReparacao
            {
                DescricaoProblema = dto.DescricaoProblema,
                VeiculoId = dto.VeiculoId,
                ClienteId = dto.ClienteId,
                DataEntrada = DateTime.UtcNow,
                Estado = "Em Curso",
                CustoMaoDeObra = dto.CustoMaoDeObra,
                CustoPecas = totalCustoPecas
            };

            _contexto.OrdensReparacao.Add(ordem);
            await _contexto.SaveChangesAsync();

            return CreatedAtAction(nameof(ObterPorId), new { id = ordem.Id }, MapearParaRespostaDto(ordem));
        }

        // RF8: PUT /api/OrdensReparacao/{id} (Atualizar Estado / Concluir)
        [HttpPut("{id}")]
        public async Task<IActionResult> AtualizarOrdem(int id, [FromBody] AtualizarOrdemReparacaoDto dto)
        {
            var ordem = await _contexto.OrdensReparacao.FindAsync(id);
            if (ordem == null)
            {
                return NotFound($"Ordem de reparação #{id} não encontrada.");
            }

            if (!string.IsNullOrEmpty(dto.Estado))
            {
                ordem.Estado = dto.Estado;
                if (dto.Estado.Equals("Concluída", StringComparison.OrdinalIgnoreCase))
                {
                    ordem.DataConclusao = DateTime.UtcNow;
                }
            }

            ordem.CustoMaoDeObra = dto.CustoMaoDeObra;
            ordem.CustoPecas = dto.CustoPecas;

            await _contexto.SaveChangesAsync();
            return Ok(MapearParaRespostaDto(ordem));
        }

        // Obter Ordem por ID
        [HttpGet("{id}")]
        public async Task<IActionResult> ObterPorId(int id)
        {
            var ordem = await _contexto.OrdensReparacao.FindAsync(id);
            if (ordem == null) return NotFound();

            return Ok(MapearParaRespostaDto(ordem));
        }

        // RF9: Consultar Histórico por Veículo
        [HttpGet("veiculo/{veiculoId}")]
        public async Task<IActionResult> ObterHistoricoPorVeiculo(int veiculoId)
        {
            var ordens = await _contexto.OrdensReparacao
                .Where(o => o.VeiculoId == veiculoId)
                .OrderByDescending(o => o.DataEntrada)
                .ToListAsync();

            return Ok(ordens.Select(MapearParaRespostaDto));
        }

        // RF9: Consultar Histórico por Cliente
        [HttpGet("cliente/{clienteId}")]
        public async Task<IActionResult> ObterHistoricoPorCliente(string clienteId)
        {
            var ordens = await _contexto.OrdensReparacao
                .Where(o => o.ClienteId == clienteId)
                .OrderByDescending(o => o.DataEntrada)
                .ToListAsync();

            return Ok(ordens.Select(MapearParaRespostaDto));
        }

        private static RespostaOrdemReparacaoDto MapearParaRespostaDto(OrdemReparacao ordem)
        {
            return new RespostaOrdemReparacaoDto
            {
                Id = ordem.Id,
                DataEntrada = ordem.DataEntrada,
                DataConclusao = ordem.DataConclusao,
                DescricaoProblema = ordem.DescricaoProblema,
                Estado = ordem.Estado,
                CustoMaoDeObra = ordem.CustoMaoDeObra,
                CustoPecas = ordem.CustoPecas,
                ValorTotal = ordem.ValorTotal,
                VeiculoId = ordem.VeiculoId,
                ClienteId = ordem.ClienteId
            };
        }
    }
}