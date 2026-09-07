using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkShop.API.Services;

namespace WorkShop.API.Controllers
{
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

        [HttpGet("pecas")]
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