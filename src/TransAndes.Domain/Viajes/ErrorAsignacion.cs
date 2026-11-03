namespace TransAndes.Domain.Viajes;

/// <summary>Tipo de incumplimiento al asignar: define la respuesta HTTP (422 o 409, contracts/api.yaml).</summary>
public enum TipoErrorAsignacion
{
    /// <summary>Documentos vencidos o capacidad insuficiente → 422.</summary>
    Validacion,

    /// <summary>Vehículo o conductor ocupado en otro viaje → 409.</summary>
    NoDisponible,
}

/// <summary>Un motivo por el que no se puede asignar un recurso.</summary>
public sealed record ErrorAsignacion(string Codigo, string Mensaje, TipoErrorAsignacion Tipo);
