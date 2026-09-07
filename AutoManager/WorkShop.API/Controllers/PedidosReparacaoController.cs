using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WorkShop.API.DTOs;
using WorkShop.API.Services;
using WorkShop.API.Services.Pedidos;

namespace WorkShop.API.Controllers
{
    [ApiController]
    [Route("api/pedidos")]
    [Authorize]
    public class PedidosReparacaoController : ControllerBase
    {
        private readonly IPedidoReparacaoService _service;

        public PedidosReparacaoController(IPedidoReparacaoService service)
        {
            _service = service;
        }

        [HttpPost]
        [Authorize(Roles = "Cliente,cliente")]
        public async Task<IActionResult> Criar([FromBody] CriarPedidoDto dto)
        {
            var clienteId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(clienteId))
            {
                return Unauthorized(new { mensagem = "Cliente não identificado no token." });
            }

            try
            {
                var resultado = await _service.CriarPedidoAsync(clienteId, dto);
                return CreatedAtRoute("ObterPendentes", new { id = resultado.Id }, resultado);
            }
            catch (UnauthorizedAccessException ex)
            {
                return BadRequest(new { mensagem = ex.Message });
            }
        }

        [HttpGet("pendentes", Name = "ObterPendentes")]
        [Authorize(Roles = "Admin,admin,Mecanico,mecanico")]
        public async Task<IActionResult> ObterPendentes()
        {
            var pendentes = await _service.ObterPendentesAsync();
            return Ok(pendentes);
        }

        [HttpPatch("{id}/analisar")]
        [Authorize(Roles = "Admin,admin,Mecanico,mecanico")]
        public async Task<IActionResult> AnalisarPedido(int id, [FromBody] AnalisarPedidoDto dto)
        {
            var atualizado = await _service.AtualizarEstadoAsync(id, dto.Estado, dto.Observacoes);
            if (atualizado == null)
            {
                return NotFound(new { mensagem = "Pedido não encontrado." });
            }
            return Ok(atualizado);
        }

        [HttpGet("meus-pedidos")]
        [Authorize(Roles = "Cliente,cliente")]
        public async Task<IActionResult> ObterMeusPedidos()
        {
            var clienteId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(clienteId))
            {
                return Unauthorized(new { mensagem = "Cliente não identificado no token." });
            }

            var pedidos = await _service.ObterPedidosDoClienteAsync(clienteId);
            return Ok(pedidos);
        }

        [HttpGet("todos")]
        [Authorize(Roles = "Admin,admin,Mecanico,mecanico")]
        public async Task<IActionResult> ObterTodos()
        {
            var pedidos = await _service.ObterTodosAsync();
            return Ok(pedidos);
        }
    }
}