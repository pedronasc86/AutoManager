using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WorkShop.API.DTOs;
using WorkShop.API.Services;
using WorkShop.API.Services.Pedidos;

namespace WorkShop.API.Controllers
{
    /// <summary>
    /// Permite criar e acompanhar pedidos de reparação submetidos pelos clientes.
    /// </summary>
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

        /// <summary>
        /// Cria um pedido de reparação para o cliente autenticado.
        /// </summary>
        /// <param name="dto">Dados do pedido de reparação.</param>
        /// <returns>O pedido criado.</returns>
        [HttpPost]
        [Authorize(Roles = "Cliente,cliente")]
        [ProducesResponseType(typeof(PedidoReparacaoResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
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

        /// <summary>
        /// Lista os pedidos de reparação que aguardam análise pela oficina.
        /// </summary>
        /// <returns>Uma coleção de pedidos pendentes.</returns>
        [HttpGet("pendentes", Name = "ObterPendentes")]
        [Authorize(Roles = "Admin,admin,Mecanico,mecanico")]
        [ProducesResponseType(typeof(IEnumerable<PedidoReparacaoResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> ObterPendentes()
        {
            var pendentes = await _service.ObterPendentesAsync();
            return Ok(pendentes);
        }

        /// <summary>
        /// Regista a decisão e as observações resultantes da análise de um pedido.
        /// </summary>
        /// <param name="id">Identificador do pedido a analisar.</param>
        /// <param name="dto">Novo estado e observações do pedido.</param>
        /// <returns>O pedido atualizado.</returns>
        [HttpPatch("{id}/analisar")]
        [Authorize(Roles = "Admin,admin,Mecanico,mecanico")]
        [ProducesResponseType(typeof(PedidoReparacaoResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> AnalisarPedido(int id, [FromBody] AnalisarPedidoDto dto)
        {
            var atualizado = await _service.AtualizarEstadoAsync(id, dto.Estado, dto.Observacoes);
            if (atualizado == null)
            {
                return NotFound(new { mensagem = "Pedido não encontrado." });
            }
            return Ok(atualizado);
        }

        /// <summary>
        /// Lista os pedidos de reparação pertencentes ao cliente autenticado.
        /// </summary>
        /// <returns>Uma coleção dos pedidos do cliente.</returns>
        [HttpGet("meus-pedidos")]
        [Authorize(Roles = "Cliente,cliente")]
        [ProducesResponseType(typeof(IEnumerable<PedidoReparacaoResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
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

        /// <summary>
        /// Lista todos os pedidos de reparação para gestão interna da oficina.
        /// </summary>
        /// <returns>Uma coleção de todos os pedidos registados.</returns>
        [HttpGet("todos")]
        [Authorize(Roles = "Admin,admin,Mecanico,mecanico")]
        [ProducesResponseType(typeof(IEnumerable<PedidoReparacaoResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> ObterTodos()
        {
            var pedidos = await _service.ObterTodosAsync();
            return Ok(pedidos);
        }
    }
}
