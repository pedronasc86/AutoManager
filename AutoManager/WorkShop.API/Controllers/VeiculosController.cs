using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorkShop.API.Data;
using WorkShop.API.DTOs;
using WorkShop.API.Models;
using WorkShop.API.Services.Auth;

namespace WorkShop.API.Controllers
{
    /// <summary>
    /// Gere os veículos registados pelos clientes da oficina.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class VeiculosController : ControllerBase
    {
        private readonly WorkshopContext _contexto;
        private readonly IUserContextService _userContextService;

        public VeiculosController(WorkshopContext contexto, IUserContextService userContextService)
        {
            _contexto = contexto;
            _userContextService = userContextService;
        }

        /// <summary>
        /// Lista todos os veículos registados na oficina.
        /// </summary>
        /// <returns>Uma coleção de veículos ordenada por identificador.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<RespostaVeiculoDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ObterTodos()
        {
            var veiculos = await _contexto.Veiculos
                .AsNoTracking()
                .OrderBy(v => v.Id)
                .Select(v => new RespostaVeiculoDto
                {
                    Id = v.Id,
                    Matricula = v.Matricula,
                    Marca = v.Marca,
                    Modelo = v.Modelo,
                    Ano = v.Ano,
                    ClienteId = v.ClienteId
                })
                .ToListAsync();

            return Ok(veiculos);
        }

        /// <summary>
        /// Cria um veículo e associa-o ao utilizador autenticado.
        /// </summary>
        /// <param name="dto">Dados do veículo a registar.</param>
        /// <returns>O veículo criado.</returns>
        [HttpPost]
        [Authorize]
        [ProducesResponseType(typeof(RespostaVeiculoDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CriarVeiculo([FromBody] CriarVeiculoDto dto)
        {
            try
            {
                var existeMatricula = await _contexto.Veiculos
                    .AnyAsync(v => v.Matricula.ToLower() == dto.Matricula.ToLower());

                if (existeMatricula)
                {
                    return BadRequest(new { message = "Já existe um veículo registado com esta matrícula." });
                }

                var clienteId = _userContextService.GetCurrentUserId();

                if (string.IsNullOrEmpty(clienteId))
                {
                    return Unauthorized(new { message = "Não foi possível identificar o cliente através do token." });
                }

                var veiculo = new Veiculo
                {
                    Matricula = dto.Matricula,
                    Marca = dto.Marca,
                    Modelo = dto.Modelo,
                    Ano = dto.Ano,
                    ClienteId = clienteId
                };

                _contexto.Veiculos.Add(veiculo);
                await _contexto.SaveChangesAsync();

                var resposta = new RespostaVeiculoDto
                {
                    Id = veiculo.Id,
                    Matricula = veiculo.Matricula,
                    Marca = veiculo.Marca,
                    Modelo = veiculo.Modelo,
                    Ano = veiculo.Ano,
                    ClienteId = veiculo.ClienteId
                };

                return CreatedAtAction(nameof(ObterPorId), new { id = veiculo.Id }, resposta);
            }
            catch (Exception ex)
            {
                var mensagemDetalhada = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                return StatusCode(500, new { message = "Erro ao guardar na base de dados: " + mensagemDetalhada });
            }
        }

        /// <summary>
        /// Obtém um veículo através do seu identificador.
        /// </summary>
        /// <param name="id">Identificador do veículo.</param>
        /// <returns>Os dados do veículo pedido.</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(RespostaVeiculoDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ObterPorId(int id)
        {
            var veiculo = await _contexto.Veiculos.FindAsync(id);
            if (veiculo == null) return NotFound();

            return Ok(new RespostaVeiculoDto
            {
                Id = veiculo.Id,
                Matricula = veiculo.Matricula,
                Marca = veiculo.Marca,
                Modelo = veiculo.Modelo,
                Ano = veiculo.Ano,
                ClienteId = veiculo.ClienteId
            });
        }

        /// <summary>
        /// Atualiza os dados de um veículo existente.
        /// </summary>
        /// <param name="id">Identificador do veículo a atualizar.</param>
        /// <param name="dto">Novos dados do veículo.</param>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AtualizarVeiculo(int id, [FromBody] CriarVeiculoDto dto)
        {
            try
            {
                var matriculaLimpa = dto.Matricula?.Trim() ?? string.Empty;

                if (string.IsNullOrEmpty(matriculaLimpa))
                {
                    return BadRequest(new { message = "A matrícula é obrigatória." });
                }

                var existeMatriculaOutro = await _contexto.Veiculos
                    .AnyAsync(v => v.Id != id && v.Matricula.Trim().ToLower() == matriculaLimpa.ToLower());

                if (existeMatriculaOutro)
                {
                    return BadRequest(new { message = "Já existe outro veículo registado com esta matrícula." });
                }

                var veiculo = await _contexto.Veiculos.FindAsync(id);

                if (veiculo == null)
                {
                    return NotFound(new { message = "Veículo não encontrado." });
                }

                veiculo.Matricula = matriculaLimpa;
                veiculo.Marca = dto.Marca?.Trim() ?? string.Empty;
                veiculo.Modelo = dto.Modelo?.Trim() ?? string.Empty;
                veiculo.Ano = dto.Ano;

                _contexto.Veiculos.Update(veiculo);
                await _contexto.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                var mensagemDetalhada = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                return StatusCode(500, new { message = "Erro ao atualizar na base de dados: " + mensagemDetalhada });
            }
        }

        /// <summary>
        /// Lista apenas os veículos associados ao utilizador autenticado.
        /// </summary>
        /// <returns>Uma coleção dos veículos do cliente autenticado.</returns>
        [HttpGet("meus")]
        [ProducesResponseType(typeof(IEnumerable<RespostaVeiculoDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ObterMeusVeiculos()
        {
            var clienteId = _userContextService.GetCurrentUserId();

            if (string.IsNullOrEmpty(clienteId))
            {
                return Unauthorized(new { message = "Não foi possível identificar o cliente através do token." });
            }

            var veiculos = await _contexto.Veiculos
                .AsNoTracking()
                .Where(v => v.ClienteId == clienteId)
                .OrderBy(v => v.Id)
                .Select(v => new RespostaVeiculoDto
                {
                    Id = v.Id,
                    Matricula = v.Matricula,
                    Marca = v.Marca,
                    Modelo = v.Modelo,
                    Ano = v.Ano,
                    ClienteId = v.ClienteId
                })
                .ToListAsync();

            return Ok(veiculos);
        }

        /// <summary>
        /// Elimina um veículo registado.
        /// </summary>
        /// <param name="id">Identificador do veículo a eliminar.</param>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> EliminarVeiculo(int id)
        {
            var veiculo = await _contexto.Veiculos.FindAsync(id);

            if (veiculo == null)
            {
                return NotFound("Veículo não encontrado.");
            }

            _contexto.Veiculos.Remove(veiculo);
            await _contexto.SaveChangesAsync();

            return NoContent();
        }
    }
}
