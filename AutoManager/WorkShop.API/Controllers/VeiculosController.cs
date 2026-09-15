using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkShop.API.DTOs;
using WorkShop.API.Services.Veiculos;

namespace WorkShop.API.Controllers;

/// <summary>Gere os veículos registados pelos clientes da oficina.</summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class VeiculosController : ControllerBase
{
    private readonly IVeiculoService _veiculoService;

    public VeiculosController(IVeiculoService veiculoService) => _veiculoService = veiculoService;

    /// <summary>Lista todos os veículos registados na oficina.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<RespostaVeiculoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ObterTodos() => ConverterResposta(await _veiculoService.ObterTodosAsync());

    /// <summary>Cria um veículo e associa-o ao utilizador autenticado.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(RespostaVeiculoDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CriarVeiculo([FromBody] CriarVeiculoDto dto) => ConverterResposta(await _veiculoService.CriarAsync(dto));

    /// <summary>Obtém um veículo através do seu identificador.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(RespostaVeiculoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterPorId(int id) => ConverterResposta(await _veiculoService.ObterPorIdAsync(id));

    /// <summary>Atualiza os dados de um veículo existente.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AtualizarVeiculo(int id, [FromBody] CriarVeiculoDto dto) => ConverterResposta(await _veiculoService.AtualizarAsync(id, dto));

    /// <summary>Lista apenas os veículos associados ao utilizador autenticado.</summary>
    [HttpGet("meus")]
    [ProducesResponseType(typeof(IEnumerable<RespostaVeiculoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ObterMeusVeiculos() => ConverterResposta(await _veiculoService.ObterMeusAsync());

    /// <summary>Elimina um veículo registado.</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> EliminarVeiculo(int id) => ConverterResposta(await _veiculoService.EliminarAsync(id));

    private IActionResult ConverterResposta(VeiculoServiceResult resultado) => StatusCode(resultado.StatusCode, resultado.Data);
}
