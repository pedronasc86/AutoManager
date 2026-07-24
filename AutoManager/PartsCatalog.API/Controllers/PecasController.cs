using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PartsCatalog.API.DTOs;
using PartsCatalog.API.Models;
using PartsCatalog.API.Repositories;

namespace PartsCatalog.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PecasController : ControllerBase
    {
        private readonly IPecaRepository _repository;
        private readonly IMapper _mapper; // ⬅️ Injeção do AutoMapper

        public PecasController(IPecaRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        // GET: api/pecas
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<PecaResponse>>> ObterTodas()
        {
            var pecas = await _repository.ObterTodasAsync();

            var response = _mapper.Map<IEnumerable<PecaResponse>>(pecas);

            return Ok(response);
        }

        // GET: api/pecas/{id}
        [HttpGet("{id:guid}")]
        [AllowAnonymous]
        public async Task<ActionResult<PecaResponse>> ObterPorId(Guid id)
        {
            var peca = await _repository.ObterPorIdAsync(id);

            if (peca == null)
                return NotFound("Peça não encontrada.");

            var response = _mapper.Map<PecaResponse>(peca);

            return Ok(response);
        }

        // POST: api/pecas
        [HttpPost]
        public async Task<ActionResult<PecaResponse>> Criar([FromBody] CriarPecaRequest request)
        {
            // Mapeia a request para a entidade Peca
            var novaPeca = _mapper.Map<Peca>(request);

            novaPeca.Id = Guid.NewGuid();
            novaPeca.Ativo = true;

            await _repository.CriarAsync(novaPeca);

            var response = _mapper.Map<PecaResponse>(novaPeca);

            return CreatedAtAction(nameof(ObterPorId), new { id = novaPeca.Id }, response);
        }

        // PUT: api/pecas/{id}
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Atualizar(Guid id, [FromBody] AtualizarPecaRequest request)
        {
            var peca = await _repository.ObterPorIdAsync(id);

            if (peca == null)
                return NotFound("Peça não encontrada.");

            // Copia os valores do request diretamente para a peça existente na base de dados
            _mapper.Map(request, peca);

            await _repository.AtualizarAsync(peca);

            return NoContent();
        }

        // DELETE: api/pecas/{id}
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Remover(Guid id)
        {
            var peca = await _repository.ObterPorIdAsync(id);

            if (peca == null)
                return NotFound("Peça não encontrada.");

            await _repository.RemoverAsync(id);
            return NoContent();
        }

        // Endpoint privado (apenas Mecânico/Gestor) para inativação de peças
        [HttpPatch("{id:guid}/inativar")]
        [Authorize(Roles = "Mecanico,Gestor,Admin")] 
        public async Task<IActionResult> InativarPeca(Guid id)
        {
            var sucesso = await _repository.InativarAsync(id);
            if (!sucesso)
                return NotFound("Peça não encontrada.");

            return NoContent(); 
        }

        // GET: api/pecas/{id}/disponibilidade?quantidade=2
        [HttpGet("{id:guid}/disponibilidade")]
        [AllowAnonymous]
        public async Task<ActionResult<bool>> VerificarDisponibilidade(Guid id, [FromQuery] int quantidade)
        {
            if (quantidade <= 0)
                return BadRequest("A quantidade deve ser maior que zero.");

            var disponivel = await _repository.VerificarDisponibilidadeAsync(id, quantidade);

            return Ok(disponivel);
        }
    }
}