using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace TransAndes.Api.Seguridad;

/// <summary>
/// Convierte <see cref="BadHttpRequestException"/> (JSON mal formado, enum inválido, cuerpo ausente) en su
/// código real (normalmente 400) con Problem Details, en lugar de un 500. Sin detalles internos (OWASP A10:2025).
/// </summary>
public sealed class SolicitudInvalidaHandler(IProblemDetailsService problemDetails) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(httpContext);
        if (exception is not BadHttpRequestException bad)
        {
            return false;
        }

        httpContext.Response.StatusCode = bad.StatusCode;
        return await problemDetails.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = new ProblemDetails
            {
                Status = bad.StatusCode,
                Title = "Solicitud inválida",
                Detail = "El cuerpo de la solicitud no tiene el formato esperado. Revise tipos y valores permitidos en el contrato de la API.",
            },
        }).ConfigureAwait(false);
    }
}
