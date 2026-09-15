using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using PartsCatalog.API.DTOs;
using PartsCatalog.API.Services;

namespace PartsCatalog.API.Controllers
{
    /// <summary>Disponibiliza operações de consulta e gestão do catálogo de peças.</summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PecasController : ControllerBase
    {
        private readonly IPecaService _pecaService;

        public PecasController(IPecaService pecaService) => _pecaService = pecaService;

        /// <summary>Lista as peças disponíveis, com suporte a filtros, pesquisa e ordenação OData.</summary>
        [HttpGet]
        [AllowAnonymous]
        [EnableQuery]
        [ProducesResponseType(typeof(IEnumerable<PecaResponse>), StatusCodes.Status200OK)]
        public IActionResult ObterTodasOData() => Ok(_pecaService.GetPartsQuery());

        /// <summary>Obtém uma peça pelo seu identificador.</summary>
        [HttpGet("{id:guid}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(PecaResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ObterPorId(Guid id) => ConverterResposta(await _pecaService.ObterPorIdAsync(id));

        /// <summary>Cria uma nova peça no catálogo.</summary>
        [HttpPost]
        [ProducesResponseType(typeof(PecaResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Criar([FromBody] CriarPecaRequest request) => ConverterResposta(await _pecaService.CriarAsync(request));

        /// <summary>Atualiza os dados de uma peça existente.</summary>
        [HttpPut("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Atualizar(Guid id, [FromBody] AtualizarPecaRequest request) => ConverterResposta(await _pecaService.AtualizarAsync(id, request));

        /// <summary>Remove uma peça do catálogo.</summary>
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Remover(Guid id) => ConverterResposta(await _pecaService.RemoverAsync(id));

        /// <summary>Inativa uma peça descontinuada.</summary>
        [HttpPatch("{id:guid}/inativar")]
        [Authorize(Roles = "Mecanico,mecanico,Gestor,gestor,Admin,admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> InativarPeca(Guid id) => ConverterResposta(await _pecaService.InativarAsync(id));

        /// <summary>Reativa uma peça previamente inativada.</summary>
        [HttpPatch("{id:guid}/ativar")]
        [Authorize(Roles = "Mecanico,mecanico,Gestor,gestor,Admin,admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AtivarPeca(Guid id) => ConverterResposta(await _pecaService.AtivarAsync(id));

        /// <summary>Verifica se existe stock suficiente de uma peça para a quantidade pedida.</summary>
        [HttpGet("{id:guid}/disponibilidade")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> VerificarDisponibilidade(Guid id, [FromQuery] int quantidade) => ConverterResposta(await _pecaService.VerificarDisponibilidadeAsync(id, quantidade));

        /// <summary>Lista todas as peças do catálogo, incluindo as inativas.</summary>
        [HttpGet("admin/todas")]
        [Authorize(Roles = "Mecanico,mecanico,Gestor,gestor,Admin,admin")]
        [ProducesResponseType(typeof(IEnumerable<PecaResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ObterTodasAdmin() => ConverterResposta(await _pecaService.ObterTodasAdminAsync());

        private IActionResult ConverterResposta(PecaServiceResult resultado) => StatusCode(resultado.StatusCode, resultado.Data);
    }
}
