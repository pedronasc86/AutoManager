using Microsoft.AspNetCore.Mvc;

namespace Indentity.API.Middlewares;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IWebHostEnvironment _environment;

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        IWebHostEnvironment environment)
    {
        _next = next;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            // Deixa o pedido continuar normalmente para controllers/endpoints.
            await _next(context);
        }
        catch (Exception exception)
        {
            // Se ocorrer um erro não tratado, devolve uma resposta JSON organizada.
            context.Response.StatusCode =
                StatusCodes.Status500InternalServerError;

            var problemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Ocorreu um erro interno no servidor.",
                Type = "https://httpstatuses.com/500",
                Instance = context.Request.Path,

                // Em desenvolvimento mostra a causa; em produção não expõe detalhes.
                Detail = _environment.IsDevelopment()
                    ? exception.Message
                    : null
            };

            await context.Response.WriteAsJsonAsync(problemDetails);
        }
    }
}
