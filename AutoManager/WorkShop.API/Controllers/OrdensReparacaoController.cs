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
        public async Task<IActionResult> ObterTodas(
            [FromQuery] int pagina = 1,
            [FromQuery] int tamanhoPagina = 5,
            [FromQuery] int? veiculoId = null)
        {
            if (veiculoId.HasValue && veiculoId.Value <= 0)
            {
                return BadRequest(new
                {
                    mensagem = "O ID do veículo deve ser maior do que zero."
                });
            }

            pagina = Math.Max(pagina, 1);
            tamanhoPagina = Math.Clamp(tamanhoPagina, 1, 20);

            // Estatísticas gerais: não mudam quando se filtra a tabela.
            var totalOrdens = await _contexto.OrdensReparacao.CountAsync();
            var totalEmCurso = await _contexto.OrdensReparacao
                .CountAsync(o => o.Estado == "Em Curso");
            var totalConcluidas = await _contexto.OrdensReparacao
                .CountAsync(o => o.Estado == "Concluída");

            // Consulta usada apenas pela tabela.
            var query = _contexto.OrdensReparacao.AsNoTracking();

            if (veiculoId.HasValue)
            {
                query = query.Where(o => o.VeiculoId == veiculoId.Value);
            }

            var totalItens = await query.CountAsync();

            // Ordem crescente: #1, #2, #3, #4, #5...
            var ordens = await query
                .OrderBy(o => o.Id)
                .Skip((pagina - 1) * tamanhoPagina)
                .Take(tamanhoPagina)
                .ToListAsync();

            var totalPaginas = Math.Max(
                1,
                (int)Math.Ceiling(totalItens / (double)tamanhoPagina)
            );

            return Ok(new RespostaPaginadaOrdensDto
            {
                Itens = ordens.Select(MapearParaRespostaDto).ToList(),
                PaginaAtual = pagina,
                TotalPaginas = totalPaginas,
                TotalItens = totalItens,
                TotalOrdens = totalOrdens,
                TotalEmCurso = totalEmCurso,
                TotalConcluidas = totalConcluidas
            });
        }

        // 2. POST: api/OrdensReparacao (Compatível com /repair-order do enunciado RF8)
        [HttpPost]
        [HttpPost("repair-order")]
        public async Task<IActionResult> CriarOrdem([FromBody] CriarOrdemReparacaoDto dto)
        {
            // Validar veículo
            var veiculo = await _contexto.Veiculos
                .AsNoTracking()
                .FirstOrDefaultAsync(v => v.Id == dto.VeiculoId);

            if (veiculo == null)
            {
                return BadRequest(new
                {
                    mensagem = $"Veículo com ID {dto.VeiculoId} não foi encontrado."
                });
            }

            if (!string.Equals(veiculo.ClienteId, dto.ClienteId, StringComparison.Ordinal))
            {
                return BadRequest(new
                {
                    mensagem = "O veículo indicado não pertence ao cliente indicado."
                });
            }

            decimal totalCustoPecas = 0;

            var pecasDaOrdem = new List<PecaAplicadaOrdem>();

            if (dto.Pecas != null && dto.Pecas.Count > 0)
            {
                foreach (var itemPeca in dto.Pecas)
                {
                    var resultadoPeca = await _catalogoPecasService
                        .VerificarStockEObterPrecoAsync(itemPeca.PecaId, itemPeca.Quantidade);

                    if (!resultadoPeca.TemStock)
                    {
                        return BadRequest(new
                        {
                            mensagem = $"Falha na validação das peças: {resultadoPeca.MensagemErro}"
                        });
                    }

                    totalCustoPecas += resultadoPeca.PrecoUnitario * itemPeca.Quantidade;

                    pecasDaOrdem.Add(new PecaAplicadaOrdem
                    {
                        PecaId = Guid.Parse(itemPeca.PecaId),
                        Quantidade = itemPeca.Quantidade,
                        PrecoUnitario = resultadoPeca.PrecoUnitario
                    });
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
                CustoPecas = totalCustoPecas,
                Pecas = pecasDaOrdem
            };

            _contexto.OrdensReparacao.Add(ordem);
            await _contexto.SaveChangesAsync();

            return CreatedAtAction(nameof(ObterPorId), new { id = ordem.Id }, MapearParaRespostaDto(ordem));
        }

        // 3. GET: api/OrdensReparacao/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> ObterPorId(int id)
        {
            var ordem = await _contexto.OrdensReparacao
                .Include(o => o.Pecas)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (ordem == null)
            {
                return NotFound();
            }

            return Ok(MapearParaDetalheDto(ordem));
        }

        // 4. PUT: api/OrdensReparacao/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "Mecanico,mecanico,Admin,admin")]
        public async Task<IActionResult> AtualizarOrdem(int id, [FromBody] AtualizarOrdemReparacaoDto dto)
        {
            var ordem = await _contexto.OrdensReparacao.FindAsync(id);
            if (ordem == null)
            {
                return NotFound(new { mensagem = $"Ordem de reparação #{id} não encontrada." });
            }

            if (!string.IsNullOrWhiteSpace(dto.Estado))
            {
                if (dto.Estado != "Em Curso" && dto.Estado != "Concluída")
                {
                    return BadRequest(new
                    {
                        mensagem = "O estado deve ser 'Em Curso' ou 'Concluída'."
                    });
                }

                ordem.Estado = dto.Estado;
                ordem.DataConclusao = dto.Estado == "Concluída"
                    ? DateTime.UtcNow
                    : null;
            }

            if (dto.CustoMaoDeObra.HasValue)
            {
                ordem.CustoMaoDeObra = dto.CustoMaoDeObra.Value;
            }

            if (dto.CustoPecas.HasValue)
            {
                ordem.CustoPecas = dto.CustoPecas.Value;
            }

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

        // 6. GET: api/OrdensReparacao/cliente/{clienteId} (Atende ao RF9)
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
        private static DetalheOrdemReparacaoDto MapearParaDetalheDto(OrdemReparacao ordem)
        {
            return new DetalheOrdemReparacaoDto
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
                ClienteId = ordem.ClienteId,
                Pecas = ordem.Pecas.Select(p => new PecaAplicadaRespostaDto
                {
                    PecaId = p.PecaId,
                    Quantidade = p.Quantidade,
                    PrecoUnitario = p.PrecoUnitario
                }).ToList()
            };
        }
    }
}