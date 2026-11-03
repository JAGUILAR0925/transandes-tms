using TransAndes.Application.Abstracciones;
using TransAndes.Domain.Flota;

namespace TransAndes.Application.Consultas;

public sealed record ClienteDto(Guid Id, string Nit, string RazonSocial);

public sealed record VehiculoDto(Guid Id, string Placa, string Tipo, int CapacidadKg, Vinculacion Vinculacion, DateOnly VenceSoat, DateOnly VenceRtm, bool DocumentosVigentes);

/// <summary>
/// Vista del conductor sin datos personales completos (Ley 1581 de 2012): la cédula se enmascara
/// y el celular no se expone. El detalle completo requerirá un rol autorizado (T011, T014).
/// </summary>
public sealed record ConductorDto(Guid Id, string CedulaEnmascarada, string Nombre, string LicenciaCategoria, DateOnly VenceLicencia, bool LicenciaVigente);

public sealed record OrdenDto(Guid Id, Guid ClienteId, string Origen, string Destino, string TipoMercancia, int PesoKg, decimal VolumenM3, DateOnly FechaRequerida);

/// <summary>Consultas por id que respaldan los encabezados Location de los POST (hallazgo #5).</summary>
public sealed class ConsultasHandler(
    IClienteRepository clientes,
    IVehiculoRepository vehiculos,
    IConductorRepository conductores,
    IOrdenRepository ordenes,
    IReloj reloj)
{
    public async Task<ClienteDto?> ClienteAsync(Guid id, CancellationToken ct)
    {
        var c = await clientes.ObtenerAsync(id, ct).ConfigureAwait(false);
        return c is null ? null : new(c.Id, c.Nit, c.RazonSocial);
    }

    public async Task<VehiculoDto?> VehiculoAsync(Guid id, CancellationToken ct)
    {
        var v = await vehiculos.ObtenerAsync(id, ct).ConfigureAwait(false);
        return v is null ? null : new(v.Id, v.Placa, v.Tipo, v.CapacidadKg, v.Vinculacion, v.VenceSoat, v.VenceRtm, v.DocumentosVigentes(reloj.Hoy));
    }

    public async Task<ConductorDto?> ConductorAsync(Guid id, CancellationToken ct)
    {
        var c = await conductores.ObtenerAsync(id, ct).ConfigureAwait(false);
        return c is null ? null : new(c.Id, Enmascarar(c.Cedula), c.Nombre, c.LicenciaCategoria, c.VenceLicencia, c.LicenciaVigente(reloj.Hoy));
    }

    public async Task<OrdenDto?> OrdenAsync(Guid id, CancellationToken ct)
    {
        var o = await ordenes.ObtenerAsync(id, ct).ConfigureAwait(false);
        return o is null ? null : new(o.Id, o.ClienteId, o.Origen, o.Destino, o.TipoMercancia, o.PesoKg, o.VolumenM3, o.FechaRequerida);
    }

    /// <summary>Deja visibles solo los últimos 4 dígitos.</summary>
    public static string Enmascarar(string cedula)
    {
        ArgumentNullException.ThrowIfNull(cedula);
        return cedula.Length <= 4 ? new string('*', cedula.Length) : new string('*', cedula.Length - 4) + cedula[^4..];
    }
}
