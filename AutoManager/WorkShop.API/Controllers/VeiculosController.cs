using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorkShop.API.Data;
using WorkShop.API.DTOs;
using WorkShop.API.Models;

namespace WorkShop.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VeiculosController : ControllerBase
    {
        private readonly WorkshopContext _contexto;

        public VeiculosController(WorkshopContext contexto)
        {
            _contexto = contexto;
        }

        [HttpPost]
        public async Task<IActionResult> CriarVeiculo([FromBody] CriarVeiculoDto dto)
        {
            var veiculo = new Veiculo
            {
                Matricula = dto.Matricula,
                Marca = dto.Marca,
                Modelo = dto.Modelo,
                Ano = dto.Ano,
                ClienteId = dto.ClienteId
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
    }
}