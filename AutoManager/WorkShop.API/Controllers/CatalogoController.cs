using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkShop.API.Services;

namespace WorkShop.API.Controllers
{
    /// <summary>
    /// Expõe o catálogo de peças da API externa à aplicação da oficina.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CatalogoController : ControllerBase
    {
        private readonly CatalogoPecasService _catalogoPecasService;

        public CatalogoController(CatalogoPecasService catalogoPecasService)
        {
            _catalogoPecasService = catalogoPecasService;
        }

        /// <summary>
        /// Obtém as peças atualmente disponíveis no catálogo externo.
        /// </summary>
        /// <returns>Uma coleção de peças disponíveis.</returns>
        [HttpGet("pecas")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ObterPecas()
        {
            try
            {
                var pecas = await _catalogoPecasService.ObterPecasAsync();
                return Ok(pecas);
            }
            catch (HttpRequestException)
            {
                return Problem(
                    title: "Catálogo indisponível",
                    detail: "Não foi possível obter as peças do catálogo.",
                    statusCode: StatusCodes.Status503ServiceUnavailable
                );
            }
        }
    }
}
