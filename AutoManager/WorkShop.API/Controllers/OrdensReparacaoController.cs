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
    [Authorize] // Exige JWT Token para todas as rotas
    public class OrdensReparacaoController : ControllerBase
    {
        private readonly WorkshopContext _contexto;
        private readonly CatalogoPecasService _catalogoPecasService;

        public OrdensReparacaoController(WorkshopContext contexto, CatalogoPecasService catalogoPecasService)
        {
            _contexto = contexto;
            _catalogoPecasService = catalogoPecasService;
        }

        // 1. GET: api/OrdensReparacao (Para a tabela principal do Dashboard)
        [HttpGet]
        public async Task<IActionResult> ObterTodas()
        {
            var ordens = await _contexto.OrdensReparacao
                .OrderByDescending(o => o.DataEntrada)
                .ToListAsync();

            return Ok(ordens.Select(MapearParaRespostaDto));
        }

        // 2. POST: api/OrdensReparacao (Para criar nova ordem)
        [HttpPost]
        public async Task<IActionResult> CriarOrdem([FromBody] CriarOrdemReparacaoDto dto)
        {
            // Validar veículo
            var veiculoExiste = await _contexto.Veiculos.AnyAsync(v => v.Id == dto.VeiculoId);
            if (!veiculoExiste)
            {
                return BadRequest($"Veículo com ID {dto.VeiculoId} não foi encontrado.");
            }

            decimal totalCustoPecas = 0;

            // Validar peças com o serviço do catálogo
            if (dto.Pecas != null && dto.Pecas.Count > 0)
            {
                foreach (var itemPeca in dto.Pecas)
                {
                    var resultadoPeca = await _catalogoPecasService.VerificarStockEObterPrecoAsync(Convert.ToInt32(itemPeca.PecaId), itemPeca.Quantidade);

                    if (!resultadoPeca.TemStock)
                    {
                        return BadRequest($"Falha na validação das peças: {resultadoPeca.MensagemErro}");
                    }

                    totalCustoPecas += (resultadoPeca.PrecoUnitario * itemPeca.Quantidade);
                }
            }

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

        // 3. GET: api/OrdensReparacao/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> ObterPorId(int id)
        {
            var ordem = await _contexto.OrdensReparacao.FindAsync(id);
            if (ordem == null) return NotFound();

            return Ok(MapearParaRespostaDto(ordem));
        }

        // 4. PUT: api/OrdensReparacao/{id}
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

        // 5. GET: api/OrdensReparacao/veiculo/{veiculoId}
        [HttpGet("veiculo/{veiculoId}")]
        public async Task<IActionResult> ObterHistoricoPorVeiculo(int veiculoId)
        {
            var ordens = await _contexto.OrdensReparacao
                .Where(o => o.VeiculoId == veiculoId)
                .OrderByDescending(o => o.DataEntrada)
                .ToListAsync();

            return Ok(ordens.Select(MapearParaRespostaDto));
        }

        // 6. GET: api/OrdensReparacao/cliente/{clienteId}
        [HttpGet("cliente/{clienteId}")]
        public async Task<IActionResult> ObterHistoricoPorCliente(string clienteId)
        {
            var ordens = await _contexto.OrdensReparacao
                .Where(o => o.ClienteId == clienteId)
                .OrderByDescending(o => o.DataEntrada)
                .ToListAsync();

            return Ok(ordens.Select(MapearParaRespostaDto));
        }

        // 7. DELETE: api/OrdensReparacao/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> ApagarOrdem(int id)
        {
            var ordem = await _contexto.OrdensReparacao.FindAsync(id);
            if (ordem == null) return NotFound();

            _contexto.OrdensReparacao.Remove(ordem);
            await _contexto.SaveChangesAsync();

            return NoContent();
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