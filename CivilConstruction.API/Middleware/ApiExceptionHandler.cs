using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CivilConstruction.API.Middleware;

internal sealed class ApiExceptionHandler(ILogger<ApiExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext context,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (status, title) = exception switch
        {
            KeyNotFoundException => (StatusCodes.Status404NotFound, "Recurso não encontrado"),
            ArgumentException => (StatusCodes.Status400BadRequest, "Requisição inválida"),
            DbUpdateConcurrencyException => (StatusCodes.Status409Conflict, "Conflito de concorrência"),
            InvalidOperationException => (StatusCodes.Status409Conflict, "Operação não permitida"),
            _ => (0, string.Empty)
        };

        if (status == 0) return false;

        logger.LogWarning(exception, "Requisição rejeitada: {Title}", title);
        context.Response.StatusCode = status;
        await context.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = title,
            Instance = context.Request.Path
        }, cancellationToken);
        return true;
    }
}
