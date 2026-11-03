using TransAndes.Application.Clientes;
using TransAndes.Application.Despacho;
using TransAndes.Application.Flota;
using TransAndes.Domain.Flota;

namespace TransAndes.Api.Endpoints;

public sealed record ClienteNuevo(string Nit, string RazonSocial);

public sealed record VehiculoNuevo(string Placa, string Tipo, int CapacidadKg, Vinculacion Vinculacion, DateOnly VenceSoat, DateOnly VenceRtm);

public sealed record ConductorNuevo(string Cedula, string Nombre, string Celular, string LicenciaCategoria, DateOnly VenceLicencia);

public sealed record OrdenNueva(Guid ClienteId, string Origen, string Destino, string TipoMercancia, int PesoKg, decimal VolumenM3, DateOnly FechaRequerida);

public sealed record ViajeNuevo(Guid OrdenId);

public sealed record Asignacion(Guid VehiculoId, Guid ConductorId);

public sealed record Creado(Guid Id);

/// <summary>Endpoints de US1 (contracts/api.yaml). Sin autenticación todavía: se agrega en T011.</summary>
public static class DespachoEndpoints
{
    public static IEndpointRouteBuilder MapDespacho(this IEndpointRouteBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);
        var v1 = app.MapGroup("/v1");

        v1.MapPost("/clientes", async (ClienteNuevo body, RegistrarClienteHandler h, CancellationToken ct) =>
        {
            var r = await h.HandleAsync(new(body.Nit, body.RazonSocial), ct).ConfigureAwait(false);
            return r.Exito ? Results.Created($"/v1/clientes/{r.Valor}", new Creado(r.Valor)) : RespuestasHttp.Problema(r);
        }).WithTags("Clientes");

        v1.MapPost("/vehiculos", async (VehiculoNuevo body, RegistrarVehiculoHandler h, CancellationToken ct) =>
        {
            var r = await h.HandleAsync(new(body.Placa, body.Tipo, body.CapacidadKg, body.Vinculacion, body.VenceSoat, body.VenceRtm), ct).ConfigureAwait(false);
            return r.Exito ? Results.Created($"/v1/vehiculos/{r.Valor}", new Creado(r.Valor)) : RespuestasHttp.Problema(r);
        }).WithTags("Flota");

        v1.MapPost("/conductores", async (ConductorNuevo body, RegistrarConductorHandler h, CancellationToken ct) =>
        {
            var r = await h.HandleAsync(new(body.Cedula, body.Nombre, body.Celular, body.LicenciaCategoria, body.VenceLicencia), ct).ConfigureAwait(false);
            return r.Exito ? Results.Created($"/v1/conductores/{r.Valor}", new Creado(r.Valor)) : RespuestasHttp.Problema(r);
        }).WithTags("Flota");

        v1.MapPost("/ordenes", async (OrdenNueva body, CrearOrdenHandler h, CancellationToken ct) =>
        {
            var r = await h.HandleAsync(new(body.ClienteId, body.Origen, body.Destino, body.TipoMercancia, body.PesoKg, body.VolumenM3, body.FechaRequerida), ct).ConfigureAwait(false);
            return r.Exito ? Results.Created($"/v1/ordenes/{r.Valor}", new Creado(r.Valor)) : RespuestasHttp.Problema(r);
        }).WithTags("Despacho");

        v1.MapPost("/viajes", async (ViajeNuevo body, CrearViajeHandler h, CancellationToken ct) =>
        {
            var r = await h.HandleAsync(new(body.OrdenId), ct).ConfigureAwait(false);
            return r.Exito ? Results.Created($"/v1/viajes/{r.Valor!.Id}", r.Valor) : RespuestasHttp.Problema(r);
        }).WithTags("Despacho");

        v1.MapGet("/viajes/{id:guid}", async (Guid id, ObtenerViajeHandler h, CancellationToken ct) =>
        {
            var viaje = await h.HandleAsync(id, ct).ConfigureAwait(false);
            return viaje is null ? Results.NotFound() : Results.Ok(viaje);
        }).WithTags("Despacho");

        v1.MapPut("/viajes/{id:guid}/asignacion", async (Guid id, Asignacion body, AsignarRecursosHandler h, CancellationToken ct) =>
        {
            var r = await h.HandleAsync(new(id, body.VehiculoId, body.ConductorId), ct).ConfigureAwait(false);
            return r.Exito ? Results.Ok(r.Valor) : RespuestasHttp.Problema(r);
        }).WithTags("Despacho");

        return app;
    }
}
