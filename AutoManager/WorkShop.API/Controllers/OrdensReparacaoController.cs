using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkShop.API.DTOs;
using WorkShop.API.Services.Ordens;

namespace WorkShop.API.Controllers;

/// <summary>Gere as ordens de reparação registadas pela oficina.</summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdensReparacaoController : ControllerBase
{
    private readonly IOrdensReparacaoService _ordensService;

    public OrdensReparacaoController(IOrdensReparacaoService ordensService)
    {
        _ordensService = ordensService;
    }

    /// <summary>Lista ordens de reparação de forma paginada e permite filtrar por veículo.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(RespostaPaginadaOrdensDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ObterTodas([FromQuery] int pagina = 1, [FromQuery] int tamanhoPagina = 5, [FromQuery] int? veiculoId = null)
        => ConverterResposta(await _ordensService.ObterTodasAsync(pagina, tamanhoPagina, veiculoId));

    /// <summary>Cria uma ordem de reparação e valida o stock das peças aplicadas.</summary>
    [HttpPost]
    [HttpPost("repair-order")]
    [Authorize(Roles = "Mecanico,mecanico,Admin,admin")]
    [ProducesResponseType(typeof(DetalheOrdemReparacaoDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CriarOrdem([FromBody] CriarOrdemReparacaoDto dto)
        => ConverterResposta(await _ordensService.CriarAsync(dto));

    /// <summary>Obtém o detalhe de uma ordem de reparação, incluindo as peças aplicadas.</summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(DetalheOrdemReparacaoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterPorId(int id)
        => ConverterResposta(await _ordensService.ObterPorIdAsync(id));

    /// <summary>Atualiza o estado, a descrição ou os custos de uma ordem de reparação.</summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Mecanico,mecanico,Admin,admin")]
    [ProducesResponseType(typeof(RespostaOrdemReparacaoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> AtualizarOrdem(int id, [FromBody] AtualizarOrdemReparacaoDto dto)
        => ConverterResposta(await _ordensService.AtualizarAsync(id, dto));

    /// <summary>Lista o histórico de reparações de um veículo.</summary>
    [HttpGet("veiculo/{veiculoId}")]
    [ProducesResponseType(typeof(IEnumerable<RespostaOrdemReparacaoDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObterHistoricoPorVeiculo(int veiculoId)
        => ConverterResposta(await _ordensService.ObterHistoricoPorVeiculoAsync(veiculoId));

    /// <summary>Lista o histórico de reparações pertencentes a um cliente.</summary>
    [HttpGet("cliente/{clienteId}")]
    [ProducesResponseType(typeof(IEnumerable<RespostaOrdemReparacaoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ObterHistoricoPorCliente(string clienteId)
        => ConverterResposta(await _ordensService.ObterHistoricoPorClienteAsync(clienteId));

    /// <summary>Elimina uma ordem de reparação.</summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ApagarOrdem(int id)
        => ConverterResposta(await _ordensService.ApagarAsync(id));

    /// <summary>Lista todas as ordens de reparação sem paginação.</summary>
    [HttpGet("todas")]
    [ProducesResponseType(typeof(IEnumerable<RespostaOrdemReparacaoDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObterTodasSemPaginacao()
        => ConverterResposta(await _ordensService.ObterTodasSemPaginacaoAsync());

    private IActionResult ConverterResposta(OrdemServiceResult resultado)
        => StatusCode(resultado.StatusCode, resultado.Data);
}
