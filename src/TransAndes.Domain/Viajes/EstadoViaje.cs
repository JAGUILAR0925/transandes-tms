namespace TransAndes.Domain.Viajes;

/// <summary>Estados del viaje (docs/architecture/05-estados.md §5.1).</summary>
public enum EstadoViaje
{
    Planeado,
    Asignado,
    RechazadoRndc,
    Despachado,
    EnRuta,
    Entregado,
    Anulado,
}
