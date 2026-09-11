using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using PartsCatalog.API.DTOs;
using PartsCatalog.API.Models;
using PartsCatalog.API.Repositories;
using PartsCatalog.API.Services;

namespace PartsCatalog.API.Controllers
{
    /// <summary>Disponibiliza operações de consulta e gestão do catálogo de peças.</summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PecasController : ControllerBase
    {
        private readonly IPecaRepository _repository;
        private readonly IPecaService _pecaService;
        private readonly IMapper _mapper;

        public PecasController(IPecaRepository repository, IPecaService pecaService, IMapper mapper)
        {
            _repository = repository;
            _pecaService = pecaService;
            _mapper = mapper;
        }

        /// <summary>Lista as peças disponíveis, com suporte a filtros, pesquisa e ordenação OData.</summary>
        [HttpGet]
        [AllowAnonymous]
        [EnableQuery]
        [ProducesResponseType(typeof(IEnumerable<PecaResponse>), StatusCodes.Status200OK)]
        public IActionResult ObterTodasOData()
        {
            var query = _pecaService.GetPartsQuery();
            return Ok(query);
        }

        /// <summary>Obtém uma peça pelo seu identificador.</summary>
        /// <param name="id">Identificador único da peça.</param>
        [HttpGet("{id:guid}")]
        [Route("{id:guid}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(PecaResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PecaResponse>> ObterPorId(Guid id)
        {
            var peca = await _repository.ObterPorIdAsync(id);

            if (peca == null)
                return NotFound("Peça não encontrada.");

            var response = _mapper.Map<PecaResponse>(peca);

            return Ok(response);
        }

        /// <summary>Cria uma nova peça no catálogo.</summary>
        /// <param name="request">Dados da peça a criar.</param>
        [HttpPost]
        [ProducesResponseType(typeof(PecaResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<PecaResponse>> Criar([FromBody] CriarPecaRequest request)
        {
            var novaPeca = _mapper.Map<Peca>(request);

            novaPeca.Id = Guid.NewGuid();

            novaPeca.Ativo = novaPeca.StockDisponivel > 0;

            await _repository.CriarAsync(novaPeca);

            var response = _mapper.Map<PecaResponse>(novaPeca);

            return CreatedAtAction(nameof(ObterPorId), new { id = novaPeca.Id }, response);
        }

        /// <summary>Atualiza os dados de uma peça existente.</summary>
        /// <param name="id">Identificador único da peça.</param>
        /// <param name="request">Novos dados da peça.</param>
        [HttpPut("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Atualizar(Guid id, [FromBody] AtualizarPecaRequest request)
        {
            var peca = await _repository.ObterPorIdAsync(id);

            if (peca == null)
                return NotFound("Peça não encontrada.");

            _mapper.Map(request, peca);

            peca.Ativo = peca.StockDisponivel > 0;

            await _repository.AtualizarAsync(peca);

            return NoContent();
        }

        /// <summary>Remove uma peça do catálogo.</summary>
        /// <param name="id">Identificador único da peça.</param>
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Remover(Guid id)
        {
            var peca = await _repository.ObterPorIdAsync(id);

            if (peca == null)
                return NotFound("Peça não encontrada.");

            await _repository.RemoverAsync(id);
            return NoContent();
        }

        /// <summary>Inativa uma peça descontinuada, impedindo a sua utilização em novas reparações.</summary>
        /// <param name="id">Identificador único da peça.</param>
        [HttpPatch("{id:guid}/inativar")]
        [AllowAnonymous]
        [Authorize(Roles = "Mecanico,mecanico,Gestor,gestor,Admin,admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> InativarPeca(Guid id)
        {
            var sucesso = await _repository.InativarAsync(id);
            if (!sucesso)
                return NotFound("Peça não encontrada.");

            return NoContent();
        }

        /// <summary>Reativa uma peça previamente inativada.</summary>
        /// <param name="id">Identificador único da peça.</param>
        [HttpPatch("{id:guid}/ativar")]
        [AllowAnonymous]
        [Authorize(Roles = "Mecanico,mecanico,Gestor,gestor,Admin,admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AtivarPeca(Guid id)
        {
            var sucesso = await _repository.AtivarAsync(id);
            if (!sucesso)
                return NotFound("Peça não encontrada.");

            return NoContent();
        }

        /// <summary>Verifica se existe stock suficiente de uma peça para a quantidade pedida.</summary>
        /// <param name="id">Identificador único da peça.</param>
        /// <param name="quantidade">Quantidade a validar.</param>
        [HttpGet("{id:guid}/disponibilidade")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<bool>> VerificarDisponibilidade(Guid id, [FromQuery] int quantidade)
        {
            if (quantidade <= 0)
                return BadRequest("A quantidade deve ser maior que zero.");

            var disponivel = await _repository.VerificarDisponibilidadeAsync(id, quantidade);

            return Ok(disponivel);
        }

        /// <summary>Lista todas as peças do catálogo, incluindo as inativas.</summary>
        [HttpGet("admin/todas")]
        [AllowAnonymous]
        [Authorize(Roles = "Mecanico,mecanico,Gestor,gestor,Admin,admin")]
        [ProducesResponseType(typeof(IEnumerable<PecaResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<PecaResponse>>> ObterTodasAdmin()
        {
            var pecas = await _repository.ObterTodasAsync();
            var response = _mapper.Map<IEnumerable<PecaResponse>>(pecas);
            return Ok(response);
        }
    }
}
