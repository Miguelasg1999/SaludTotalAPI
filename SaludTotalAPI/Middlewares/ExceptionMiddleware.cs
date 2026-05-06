using System;
using System.Net;
using System.Text.Json;

namespace SaludTotalAPI.Middlewares;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await _next(httpContext);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "=== Ocurrió una excepción no controlada ===");

            await HandleExceptionAsync(httpContext, ex);
        }

    }

    private static Task HandleExceptionAsync(HttpContext httpContext, Exception exception)
    {
        httpContext.Response.ContentType = "application/json";

        var response = new
        {
            success = false,
            message = "Ocurrió un error interno en el servidor"
        };

        httpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        var json = JsonSerializer.Serialize(response);

        return httpContext.Response.WriteAsync(json);
    }

}
