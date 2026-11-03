using TransAndes.Application.Comun;

namespace TransAndes.Api.Endpoints;

/// <summary>Traduce resultados de casos de uso a respuestas HTTP (RFC 9457 Problem Details).</summary>
internal static class RespuestasHttp
{
    public static IResult Problema<T>(Resultado<T> resultado)
    {
        var (status, titulo) = resultado.TipoError switch
        {
            TipoError.NoEncontrado => (StatusCodes.Status404NotFound, "Recurso no encontrado"),
            TipoError.Conflicto => (StatusCodes.Status409Conflict, "Conflicto con el estado actual"),
            _ => (StatusCodes.Status422UnprocessableEntity, "La solicitud no cumple las reglas de negocio"),
        };

        return Results.Problem(
            title: titulo,
            detail: string.Join(" ", resultado.Errores.Select(e => e.Mensaje)),
            statusCode: status,
            extensions: new Dictionary<string, object?> { ["errores"] = resultado.Errores });
    }
}
