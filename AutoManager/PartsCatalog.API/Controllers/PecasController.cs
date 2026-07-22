using Microsoft.AspNetCore.Mvc;
using PartsCatalog.API.DTOs;
using PartsCatalog.API.Models;
using PartsCatalog.API.Repositories;

namespace PartsCatalog.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PecasController : ControllerBase
    {
        private readonly IPecaRepository _repository;

        public PecasController(IPecaRepository repository)
        {
            _repository = repository;
        }

        // GET: api/pecas
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PecaResponse>>> ObterTodas()
        {
            var pecas = await _repository.ObterTodasAsync();

            var response = pecas.Select(p => new PecaResponse
            {
                Id = p.Id,
                Nome = p.Nome,
                ReferenciaPeca = p.ReferenciaPeca,
                Categoria = p.Categoria,
                Compatibilidade = p.Compatibilidade,
                PrecoUnitario = p.PrecoUnitario,
                StockDisponivel = p.StockDisponivel,
                Ativo = p.Ativo
            });

            return Ok(response);
        }

        // GET: api/pecas/{id}
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<PecaResponse>> ObterPorId(Guid id)
        {
            var peca = await _repository.ObterPorIdAsync(id);

            if (peca == null)
                return NotFound("Peça não encontrada.");

            var response = new PecaResponse
            {
                Id = peca.Id,
                Nome = peca.Nome,
                ReferenciaPeca = peca.ReferenciaPeca,
                Categoria = peca.Categoria,
                Compatibilidade = peca.Compatibilidade,
                PrecoUnitario = peca.PrecoUnitario,
                StockDisponivel = peca.StockDisponivel,
                Ativo = peca.Ativo
            };

            return Ok(response);
        }

        // POST: api/pecas
        [HttpPost]
        public async Task<ActionResult<PecaResponse>> Criar([FromBody] CriarPecaRequest request)
        {
            var novaPeca = new Peca
            {
                Id = Guid.NewGuid(),
                Nome = request.Nome,
                ReferenciaPeca = request.ReferenciaPeca,
                Categoria = request.Categoria,
                Compatibilidade = request.Compatibilidade,
                PrecoUnitario = request.PrecoUnitario,
                StockDisponivel = request.StockDisponivel,
                Ativo = true
            };

            await _repository.CriarAsync(novaPeca);

            var response = new PecaResponse
            {
                Id = novaPeca.Id,
                Nome = novaPeca.Nome,
                ReferenciaPeca = novaPeca.ReferenciaPeca,
                Categoria = novaPeca.Categoria,
                Compatibilidade = novaPeca.Compatibilidade,
                PrecoUnitario = novaPeca.PrecoUnitario,
                StockDisponivel = novaPeca.StockDisponivel,
                Ativo = novaPeca.Ativo
            };

            return CreatedAtAction(nameof(ObterPorId), new { id = novaPeca.Id }, response);
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
    }
}