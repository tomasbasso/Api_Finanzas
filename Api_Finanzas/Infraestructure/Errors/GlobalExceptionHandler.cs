using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Api_Finanzas;

public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;
    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) => _logger = logger;

    public async ValueTask<bool> TryHandleAsync(HttpContext http, Exception ex, CancellationToken token)
    {
        _logger.LogError(ex, "Unhandled exception [{TraceId}]", http.TraceIdentifier);

        var status = ex switch
        {
            NotFoundException => StatusCodes.Status404NotFound,
            DomainException => StatusCodes.Status422UnprocessableEntity,
            UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
            _ => StatusCodes.Status500InternalServerError
        };

        var problem = new ProblemDetails
        {
            Status = status,
            Title = status switch
            {
                StatusCodes.Status404NotFound => "Recurso no encontrado",
                StatusCodes.Status422UnprocessableEntity => "Validación de dominio fallida",
                StatusCodes.Status401Unauthorized => "No autorizado",
                _ => "Error interno del servidor"
            },
            Detail = status == StatusCodes.Status500InternalServerError
                ? "Ocurrió un error inesperado. Intentá más tarde."
                : ex.Message,
            Instance = http.Request.Path
        };

        problem.Extensions["traceId"] = http.TraceIdentifier;

        http.Response.ContentType = "application/problem+json";
        http.Response.StatusCode = status;
        await http.Response.WriteAsJsonAsync(problem, cancellationToken: token);
        return true;
    }
}

// Excepciones de dominio (podés dejarlas acá o en un archivo aparte si preferís)
public sealed class DomainException(string message) : Exception(message);
public sealed class NotFoundException(string message) : Exception(message);
