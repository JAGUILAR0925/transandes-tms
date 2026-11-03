using TransAndes.Domain.Flota;
using TransAndes.Domain.Ordenes;

namespace TransAndes.Domain.Viajes;

/// <summary>
/// Servicio de dominio que valida si un vehículo y un conductor pueden asignarse a un viaje (FR-003, US1).
/// Devuelve todos los incumplimientos, no solo el primero, para que el coordinador los corrija de una vez.
/// </summary>
public static class PoliticaAsignacion
{
    public static IReadOnlyList<ErrorAsignacion> Validar(
        Viaje viaje,
        OrdenDeCargue orden,
        Vehiculo vehiculo,
        Conductor conductor,
        DateOnly hoy,
        bool vehiculoEnOtroViajeActivo,
        bool conductorEnOtroViajeActivo)
    {
        ArgumentNullException.ThrowIfNull(viaje);
        ArgumentNullException.ThrowIfNull(orden);
        ArgumentNullException.ThrowIfNull(vehiculo);
        ArgumentNullException.ThrowIfNull(conductor);

        var errores = new List<ErrorAsignacion>();

        if (viaje.OrdenId != orden.Id)
        {
            errores.Add(new("ORDEN_NO_CORRESPONDE", "La orden no corresponde a este viaje.", TipoErrorAsignacion.Validacion));
        }

        if (viaje.Estado != EstadoViaje.Planeado)
        {
            errores.Add(new("ESTADO_INVALIDO", $"Solo se asignan viajes en estado Planeado; este está en {viaje.Estado}.", TipoErrorAsignacion.Validacion));
        }

        foreach (var documento in vehiculo.DocumentosVencidos(hoy))
        {
            errores.Add(new("DOCUMENTO_VEHICULO_VENCIDO", $"El vehículo {vehiculo.Placa} tiene vencido: {documento}.", TipoErrorAsignacion.Validacion));
        }

        if (!conductor.LicenciaVigente(hoy))
        {
            errores.Add(new("LICENCIA_VENCIDA", $"La licencia del conductor venció el {conductor.VenceLicencia:yyyy-MM-dd}.", TipoErrorAsignacion.Validacion));
        }

        if (vehiculo.CapacidadKg < orden.PesoKg)
        {
            var faltante = orden.PesoKg - vehiculo.CapacidadKg;
            errores.Add(new("CAPACIDAD_INSUFICIENTE", $"El vehículo {vehiculo.Placa} soporta {vehiculo.CapacidadKg} kg; faltan {faltante} kg para la carga de {orden.PesoKg} kg.", TipoErrorAsignacion.Validacion));
        }

        if (vehiculoEnOtroViajeActivo)
        {
            errores.Add(new("VEHICULO_NO_DISPONIBLE", $"El vehículo {vehiculo.Placa} está asignado a otro viaje activo.", TipoErrorAsignacion.NoDisponible));
        }

        if (conductorEnOtroViajeActivo)
        {
            errores.Add(new("CONDUCTOR_NO_DISPONIBLE", "El conductor está asignado a otro viaje activo.", TipoErrorAsignacion.NoDisponible));
        }

        return errores;
    }
}
