using TransAndes.Application.Abstracciones;
using TransAndes.Application.Comun;
using TransAndes.Domain.Comun;
using TransAndes.Domain.Ordenes;
using TransAndes.Domain.Viajes;

namespace TransAndes.Application.Despacho;

public sealed record CrearOrdenCommand(Guid ClienteId, string Origen, string Destino, string TipoMercancia, int PesoKg, decimal VolumenM3, DateOnly FechaRequerida);

public sealed record CrearViajeCommand(Guid OrdenId);

public sealed record AsignarRecursosCommand(Guid ViajeId, Guid VehiculoId, Guid ConductorId);

/// <summary>Vista de un viaje para la API.</summary>
public sealed record ViajeDto(Guid Id, string Consecutivo, Guid OrdenId, string Estado, Guid? VehiculoId, Guid? ConductorId)
{
    public static ViajeDto Desde(Viaje viaje)
    {
        ArgumentNullException.ThrowIfNull(viaje);
        return new(viaje.Id, viaje.Consecutivo, viaje.OrdenId, viaje.Estado.ToString(), viaje.VehiculoId, viaje.ConductorId);
    }
}

/// <summary>FR-001 — registrar una orden de cargue de un cliente existente.</summary>
public sealed class CrearOrdenHandler(IClienteRepository clientes, IOrdenRepository ordenes)
{
    public async Task<Resultado<Guid>> HandleAsync(CrearOrdenCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);
        if (await clientes.ObtenerAsync(cmd.ClienteId, ct).ConfigureAwait(false) is null)
        {
            return Resultado.Falla<Guid>(TipoError.NoEncontrado, new ErrorDetalle("CLIENTE_NO_EXISTE", "El cliente no existe."));
        }

        try
        {
            var orden = new OrdenDeCargue(Guid.NewGuid(), cmd.ClienteId, cmd.Origen, cmd.Destino, cmd.TipoMercancia, cmd.PesoKg, cmd.VolumenM3, cmd.FechaRequerida);
            await ordenes.AgregarAsync(orden, ct).ConfigureAwait(false);
            return Resultado.Ok(orden.Id);
        }
        catch (DomainException ex)
        {
            return Resultado.Falla<Guid>(TipoError.Validacion, new ErrorDetalle("ORDEN_INVALIDA", ex.Message));
        }
    }
}

/// <summary>FR-002 — convertir una orden en un viaje Planeado con consecutivo único (MVP: una orden, un viaje).</summary>
public sealed class CrearViajeHandler(IOrdenRepository ordenes, IViajeRepository viajes)
{
    public async Task<Resultado<ViajeDto>> HandleAsync(CrearViajeCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);
        if (await ordenes.ObtenerAsync(cmd.OrdenId, ct).ConfigureAwait(false) is null)
        {
            return Resultado.Falla<ViajeDto>(TipoError.NoEncontrado, new ErrorDetalle("ORDEN_NO_EXISTE", "La orden no existe."));
        }

        if (await viajes.ExisteParaOrdenAsync(cmd.OrdenId, ct).ConfigureAwait(false))
        {
            return Resultado.Falla<ViajeDto>(TipoError.Conflicto, new ErrorDetalle("ORDEN_CON_VIAJE", "La orden ya tiene un viaje."));
        }

        var consecutivo = await viajes.SiguienteConsecutivoAsync(ct).ConfigureAwait(false);
        var viaje = new Viaje(Guid.NewGuid(), consecutivo, cmd.OrdenId);
        await viajes.AgregarAsync(viaje, ct).ConfigureAwait(false);
        return Resultado.Ok(ViajeDto.Desde(viaje));
    }
}

/// <summary>FR-003 / US1 — asignar vehículo y conductor validando documentos, capacidad y disponibilidad.</summary>
public sealed class AsignarRecursosHandler(
    IViajeRepository viajes,
    IOrdenRepository ordenes,
    IVehiculoRepository vehiculos,
    IConductorRepository conductores,
    IReloj reloj)
{
    public async Task<Resultado<ViajeDto>> HandleAsync(AsignarRecursosCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);
        var viaje = await viajes.ObtenerAsync(cmd.ViajeId, ct).ConfigureAwait(false);
        if (viaje is null)
        {
            return NoEncontrado("VIAJE_NO_EXISTE", "El viaje no existe.");
        }

        var vehiculo = await vehiculos.ObtenerAsync(cmd.VehiculoId, ct).ConfigureAwait(false);
        if (vehiculo is null)
        {
            return NoEncontrado("VEHICULO_NO_EXISTE", "El vehículo no existe.");
        }

        var conductor = await conductores.ObtenerAsync(cmd.ConductorId, ct).ConfigureAwait(false);
        if (conductor is null)
        {
            return NoEncontrado("CONDUCTOR_NO_EXISTE", "El conductor no existe.");
        }

        var orden = await ordenes.ObtenerAsync(viaje.OrdenId, ct).ConfigureAwait(false);
        if (orden is null)
        {
            return NoEncontrado("ORDEN_NO_EXISTE", "La orden del viaje no existe.");
        }

        var vehiculoOcupado = await viajes.VehiculoEnViajeActivoAsync(vehiculo.Id, viaje.Id, ct).ConfigureAwait(false);
        var conductorOcupado = await viajes.ConductorEnViajeActivoAsync(conductor.Id, viaje.Id, ct).ConfigureAwait(false);

        var errores = PoliticaAsignacion.Validar(viaje, orden, vehiculo, conductor, reloj.Hoy, vehiculoOcupado, conductorOcupado);
        if (errores.Count > 0)
        {
            // Las reglas de validación (422) prevalecen: no sirve liberar un recurso que igual no se puede usar.
            var hayValidacion = errores.Any(e => e.Tipo == TipoErrorAsignacion.Validacion);
            var tipo = hayValidacion ? TipoError.Validacion : TipoError.Conflicto;
            return Resultado.Falla<ViajeDto>(tipo, errores.Select(e => new ErrorDetalle(e.Codigo, e.Mensaje)));
        }

        viaje.Asignar(vehiculo.Id, conductor.Id);
        await viajes.ActualizarAsync(viaje, ct).ConfigureAwait(false);
        return Resultado.Ok(ViajeDto.Desde(viaje));
    }

    private static Resultado<ViajeDto> NoEncontrado(string codigo, string mensaje) =>
        Resultado.Falla<ViajeDto>(TipoError.NoEncontrado, new ErrorDetalle(codigo, mensaje));
}

/// <summary>Consulta de un viaje.</summary>
public sealed class ObtenerViajeHandler(IViajeRepository viajes)
{
    public async Task<ViajeDto?> HandleAsync(Guid id, CancellationToken ct)
    {
        var viaje = await viajes.ObtenerAsync(id, ct).ConfigureAwait(false);
        return viaje is null ? null : ViajeDto.Desde(viaje);
    }
}
