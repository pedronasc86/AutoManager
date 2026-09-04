using Microsoft.AspNetCore.Mvc;

namespace WorkShop.API.Middlewares;

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
            await _next(context);
        }
        catch (Exception exception)
        {
            context.Response.StatusCode =
                StatusCodes.Status500InternalServerError;

            var problemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Ocorreu um erro interno no servidor.",
                Type = "https://httpstatuses.com/500",
                Instance = context.Request.Path,

                // Só mostra informação técnica quando estão a desenvolver.
                Detail = _environment.IsDevelopment()
                    ? exception.Message
                    : null
            };

            await context.Response.WriteAsJsonAsync(problemDetails);
        }
    }
}
