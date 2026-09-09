using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkShop.API.DTOs;
using WorkShop.API.Services.Ordens;

namespace WorkShop.API.Controllers;

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

    [HttpGet]
    public async Task<IActionResult> ObterTodas([FromQuery] int pagina = 1, [FromQuery] int tamanhoPagina = 5, [FromQuery] int? veiculoId = null)
        => ConverterResposta(await _ordensService.ObterTodasAsync(pagina, tamanhoPagina, veiculoId));

    [HttpPost]
    [HttpPost("repair-order")]
    [Authorize(Roles = "Mecanico,mecanico,Admin,admin")]
    public async Task<IActionResult> CriarOrdem([FromBody] CriarOrdemReparacaoDto dto)
        => ConverterResposta(await _ordensService.CriarAsync(dto));

    [HttpGet("{id}")]
    public async Task<IActionResult> ObterPorId(int id)
        => ConverterResposta(await _ordensService.ObterPorIdAsync(id));

    [HttpPut("{id}")]
    [Authorize(Roles = "Mecanico,mecanico,Admin,admin")]
    public async Task<IActionResult> AtualizarOrdem(int id, [FromBody] AtualizarOrdemReparacaoDto dto)
        => ConverterResposta(await _ordensService.AtualizarAsync(id, dto));

    [HttpGet("veiculo/{veiculoId}")]
    public async Task<IActionResult> ObterHistoricoPorVeiculo(int veiculoId)
        => ConverterResposta(await _ordensService.ObterHistoricoPorVeiculoAsync(veiculoId));

    [HttpGet("cliente/{clienteId}")]
    public async Task<IActionResult> ObterHistoricoPorCliente(string clienteId)
        => ConverterResposta(await _ordensService.ObterHistoricoPorClienteAsync(clienteId));

    [HttpDelete("{id}")]
    public async Task<IActionResult> ApagarOrdem(int id)
        => ConverterResposta(await _ordensService.ApagarAsync(id));

    [HttpGet("todas")]
    public async Task<IActionResult> ObterTodasSemPaginacao()
        => ConverterResposta(await _ordensService.ObterTodasSemPaginacaoAsync());

    private IActionResult ConverterResposta(OrdemServiceResult resultado)
        => StatusCode(resultado.StatusCode, resultado.Data);
}
