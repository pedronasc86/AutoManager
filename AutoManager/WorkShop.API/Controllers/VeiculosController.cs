using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorkShop.API.Data;
using WorkShop.API.DTOs;
using WorkShop.API.Models;
using WorkShop.API.Services.Auth;

namespace WorkShop.API.Controllers
{
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

        [HttpGet]
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

        [HttpPost]
        [Authorize]
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

        [HttpGet("{id}")]
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

        [HttpPut("{id}")]
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

        [HttpGet("meus")]
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

        [HttpDelete("{id}")]
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