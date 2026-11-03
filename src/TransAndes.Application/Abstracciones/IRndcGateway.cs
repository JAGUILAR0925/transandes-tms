namespace TransAndes.Application.Abstracciones;

/// <summary>
/// Puerto hacia el RNDC (ADR-002). El adaptador real vive en TransAndes.Rndc y
/// depende de la aclaración pendiente sobre el mecanismo técnico oficial.
/// </summary>
public interface IRndcGateway
{
    Task<RespuestaRndc> EnviarManifiestoAsync(Guid viajeId, CancellationToken cancellationToken);

    Task<RespuestaRndc> AnularAsync(string numeroManifiesto, CancellationToken cancellationToken);

    /// <summary>Consulta por clave de idempotencia antes de reenviar (05-estados.md §5.2).</summary>
    Task<RespuestaRndc?> ConsultarAsync(Guid viajeId, CancellationToken cancellationToken);
}

public sealed record RespuestaRndc(bool Aceptado, string? NumeroManifiesto, string? Error, bool EsTransitorio);
