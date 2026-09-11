using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorkShop.API.Data;
using WorkShop.API.DTOs;
using WorkShop.API.Models;
using WorkShop.API.Services;
using WorkShop.API.Services.Auth;
using WorkShop.API.Services.Ordens;

namespace WorkShop.API.Controllers
{
    /// <summary>Gere as ordens de reparação registadas pela oficina.</summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Exige JWT Token para todas as rotas
    public class OrdensReparacaoController : ControllerBase
    {
        private readonly WorkshopContext _contexto;
        private readonly CatalogoPecasService _catalogoPecasService;
        private readonly IUserContextService _userContextService;
        private readonly IOrdensReparacaoService _ordensService;

        public OrdensReparacaoController(
            WorkshopContext contexto,
            CatalogoPecasService catalogoPecasService,
            IUserContextService userContextService,
            IOrdensReparacaoService ordensService)
        {
            _contexto = contexto;
            _catalogoPecasService = catalogoPecasService;
            _userContextService = userContextService;
            _ordensService = ordensService;
        }

        /// <summary>Lista ordens de reparação de forma paginada e permite filtrar por veículo.</summary>
        [HttpGet]
        [ProducesResponseType(typeof(RespostaPaginadaOrdensDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
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

        /// <summary>Cria uma ordem de reparação e valida o stock das peças aplicadas.</summary>
        [HttpPost]
        [HttpPost("repair-order")]
        [Authorize(Roles = "Mecanico,mecanico,Admin,admin")]
        [ProducesResponseType(typeof(DetalheOrdemReparacaoDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
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
                DataConclusao = DateTime.UtcNow,
                Estado = "Concluída",
                CustoMaoDeObra = dto.CustoMaoDeObra,
                CustoPecas = totalCustoPecas,
                Pecas = pecasDaOrdem
            };

            _contexto.OrdensReparacao.Add(ordem);
            await _contexto.SaveChangesAsync();

            // Reduzir o stock das peças no microsserviço de catálogo após gravar a ordem com sucesso
            if (dto.Pecas != null && dto.Pecas.Count > 0)
            {
                foreach (var itemPeca in dto.Pecas)
                {
                    await _catalogoPecasService.AtualizarStockAsync(itemPeca.PecaId, itemPeca.Quantidade);
                }
            }

            return CreatedAtAction(nameof(ObterPorId), new { id = ordem.Id }, MapearParaDetalheDto(ordem));
        }

        /// <summary>Obtém o detalhe de uma ordem de reparação, incluindo as peças aplicadas.</summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(DetalheOrdemReparacaoDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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

        /// <summary>Atualiza o estado, a descrição ou os custos de uma ordem de reparação.</summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Mecanico,mecanico,Admin,admin")]
        [ProducesResponseType(typeof(RespostaOrdemReparacaoDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
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

            if (!string.IsNullOrWhiteSpace(dto.DescricaoProblema))
            {
                ordem.DescricaoProblema = dto.DescricaoProblema.Trim();
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

        /// <summary>Lista o histórico de reparações de um veículo.</summary>
        [HttpGet("veiculo/{veiculoId}")]
        [ProducesResponseType(typeof(IEnumerable<RespostaOrdemReparacaoDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ObterHistoricoPorVeiculo(int veiculoId)
        {
            var ordens = await _contexto.OrdensReparacao
                .Where(o => o.VeiculoId == veiculoId)
                .OrderByDescending(o => o.DataEntrada)
                .ToListAsync();

            return Ok(ordens.Select(MapearParaRespostaDto));
        }

        /// <summary>Lista o histórico de reparações pertencentes a um cliente.</summary>
        [HttpGet("cliente/{clienteId}")]
        [ProducesResponseType(typeof(IEnumerable<RespostaOrdemReparacaoDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> ObterHistoricoPorCliente(string clienteId)
        {
            var utilizadorAutenticadoId = _userContextService.GetCurrentUserId();

            if (string.IsNullOrWhiteSpace(utilizadorAutenticadoId))
            {
                return Unauthorized();
            }

            if (!string.Equals(utilizadorAutenticadoId, clienteId, StringComparison.Ordinal))
            {
                return Forbid();
            }

            var ordens = await _contexto.OrdensReparacao
                .Where(o => o.ClienteId == clienteId)
                .OrderByDescending(o => o.DataEntrada)
                .ToListAsync();

            return Ok(ordens.Select(MapearParaRespostaDto));
        }

        /// <summary>Elimina uma ordem de reparação.</summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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

        /// <summary>Lista todas as ordens de reparação sem paginação.</summary>
        [HttpGet("todas")]
        [ProducesResponseType(typeof(IEnumerable<RespostaOrdemReparacaoDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ObterTodasSemPaginacao()
        {
            var ordens = await _contexto.OrdensReparacao
                .AsNoTracking()
                .OrderByDescending(o => o.Id)
                .ToListAsync();

            return Ok(ordens.Select(MapearParaRespostaDto));
        }
    }
}