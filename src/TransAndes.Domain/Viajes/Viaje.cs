using TransAndes.Domain.Comun;

namespace TransAndes.Domain.Viajes;

/// <summary>
/// Raíz de agregado del viaje. Implementa la máquina de estados de
/// docs/architecture/05-estados.md §5.1 y las invariantes de 04-clases.md.
/// </summary>
public sealed class Viaje
{
    public Guid Id { get; }
    public string Consecutivo { get; }
    public Guid OrdenId { get; }
    public Guid? VehiculoId { get; private set; }
    public Guid? ConductorId { get; private set; }
    public EstadoViaje Estado { get; private set; }
    public string? NumeroManifiesto { get; private set; }
    public string? UltimoErrorRndc { get; private set; }

    /// <summary>Un viaje activo ocupa su vehículo y su conductor (FR-003).</summary>
    public bool EstaActivo => Estado is EstadoViaje.Asignado or EstadoViaje.RechazadoRndc or EstadoViaje.Despachado or EstadoViaje.EnRuta;

    public Viaje(Guid id, string consecutivo, Guid ordenId)
    {
        if (string.IsNullOrWhiteSpace(consecutivo))
        {
            throw new DomainException("El consecutivo del viaje es obligatorio.");
        }

        Id = id;
        Consecutivo = consecutivo;
        OrdenId = ordenId;
        Estado = EstadoViaje.Planeado;
    }

    /// <summary>Planeado → Asignado. La validación de documentos y capacidad la hace PoliticaAsignacion (FR-003).</summary>
    public void Asignar(Guid vehiculoId, Guid conductorId)
    {
        ExigirEstado(EstadoViaje.Planeado);
        VehiculoId = vehiculoId;
        ConductorId = conductorId;
        Estado = EstadoViaje.Asignado;
    }

    /// <summary>Asignado → Planeado.</summary>
    public void Desasignar()
    {
        ExigirEstado(EstadoViaje.Asignado);
        VehiculoId = null;
        ConductorId = null;
        Estado = EstadoViaje.Planeado;
    }

    /// <summary>Asignado → Despachado. Solo con manifiesto aceptado por el RNDC (Principio II).</summary>
    public void MarcarDespachado(string numeroManifiesto)
    {
        ExigirEstado(EstadoViaje.Asignado);
        if (string.IsNullOrWhiteSpace(numeroManifiesto))
        {
            throw new DomainException("No se puede despachar sin número de manifiesto del RNDC.");
        }

        NumeroManifiesto = numeroManifiesto;
        UltimoErrorRndc = null;
        Estado = EstadoViaje.Despachado;
    }

    /// <summary>Asignado → RechazadoRndc.</summary>
    public void MarcarRechazadoRndc(string error)
    {
        ExigirEstado(EstadoViaje.Asignado);
        UltimoErrorRndc = error;
        Estado = EstadoViaje.RechazadoRndc;
    }

    /// <summary>RechazadoRndc → Asignado, tras corregir datos para reenviar.</summary>
    public void PrepararReenvio()
    {
        ExigirEstado(EstadoViaje.RechazadoRndc);
        Estado = EstadoViaje.Asignado;
    }

    /// <summary>Despachado → EnRuta.</summary>
    public void IniciarRuta()
    {
        ExigirEstado(EstadoViaje.Despachado);
        Estado = EstadoViaje.EnRuta;
    }

    /// <summary>EnRuta → Entregado. Exige prueba de entrega completa (FR-008).</summary>
    public void Entregar(bool pruebaDeEntregaCompleta)
    {
        ExigirEstado(EstadoViaje.EnRuta);
        if (!pruebaDeEntregaCompleta)
        {
            throw new DomainException("La entrega exige fotos, datos del receptor y firma.");
        }

        Estado = EstadoViaje.Entregado;
    }

    /// <summary>
    /// → Anulado. Desde Despachado o EnRuta exige la anulación previa en el RNDC.
    /// </summary>
    public void Anular(bool anulacionRndcConfirmada = false)
    {
        switch (Estado)
        {
            case EstadoViaje.Planeado or EstadoViaje.Asignado or EstadoViaje.RechazadoRndc:
                break;
            case EstadoViaje.Despachado or EstadoViaje.EnRuta when anulacionRndcConfirmada:
                break;
            case EstadoViaje.Despachado or EstadoViaje.EnRuta:
                throw new DomainException("Anular un viaje despachado exige anularlo primero en el RNDC.");
            default:
                throw new DomainException($"No se puede anular un viaje en estado {Estado}.");
        }

        Estado = EstadoViaje.Anulado;
    }

    private void ExigirEstado(EstadoViaje esperado)
    {
        if (Estado != esperado)
        {
            throw new DomainException($"Transición inválida: se esperaba {esperado} y el viaje está en {Estado}.");
        }
    }
}
