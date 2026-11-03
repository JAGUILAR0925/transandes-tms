using TransAndes.Application.Abstracciones;
using TransAndes.Application.Clientes;
using TransAndes.Application.Comun;
using TransAndes.Application.Despacho;
using TransAndes.Application.Flota;
using TransAndes.Domain.Flota;
using TransAndes.Infrastructure.EnMemoria;

namespace TransAndes.Application.Tests.Despacho;

/// <summary>US1 — planear y despachar un viaje (escenarios de aceptación 1-4).</summary>
public class DespachoTests
{
    private static readonly DateOnly Hoy = new(2027, 1, 15);

    private sealed class RelojFijo(DateOnly hoy) : IReloj
    {
        public DateOnly Hoy { get; } = hoy;
    }

    private readonly ClienteRepositoryEnMemoria _clientes = new();
    private readonly VehiculoRepositoryEnMemoria _vehiculos = new();
    private readonly ConductorRepositoryEnMemoria _conductores = new();
    private readonly OrdenRepositoryEnMemoria _ordenes = new();
    private readonly ViajeRepositoryEnMemoria _viajes = new();

    private int _clientesCreados;

    private AsignarRecursosHandler Asignador() => new(_viajes, _ordenes, _vehiculos, _conductores, new RelojFijo(Hoy));

    private async Task<Guid> ClienteAsync()
    {
        var nit = $"900{++_clientesCreados:D6}-1";
        var r = await new RegistrarClienteHandler(_clientes).HandleAsync(new(nit, "Alimentos del Valle (ficticio)"), default);
        Assert.True(r.Exito);
        return r.Valor;
    }

    private async Task<Guid> VehiculoAsync(string placa = "TAC123", int capacidadKg = 10_000, DateOnly? soat = null) =>
        (await new RegistrarVehiculoHandler(_vehiculos).HandleAsync(
            new(placa, "Camión sencillo", capacidadKg, Vinculacion.Propio, soat ?? Hoy.AddMonths(6), Hoy.AddMonths(6)), default)).Valor;

    private async Task<Guid> ConductorAsync(string cedula = "1037000111") =>
        (await new RegistrarConductorHandler(_conductores).HandleAsync(new(cedula, "Carlos Pérez", "3001234567", "C2", Hoy.AddYears(1)), default)).Valor;

    private async Task<ViajeDto> ViajePlaneadoAsync(int pesoKg = 8_000)
    {
        var clienteId = await ClienteAsync();
        var ordenId = (await new CrearOrdenHandler(_clientes, _ordenes).HandleAsync(
            new(clienteId, "Medellín", "Barranquilla", "Alimentos", pesoKg, 30m, Hoy.AddDays(2)), default)).Valor;
        return (await new CrearViajeHandler(_ordenes, _viajes).HandleAsync(new(ordenId), default)).Valor!;
    }

    [Fact]
    public async Task Crear_viaje_desde_orden_queda_planeado_con_consecutivo()
    {
        var viaje = await ViajePlaneadoAsync();

        Assert.Equal("Planeado", viaje.Estado);
        Assert.Equal("V-000001", viaje.Consecutivo);
    }

    [Fact]
    public async Task Asignar_recursos_validos_deja_el_viaje_asignado()
    {
        var viaje = await ViajePlaneadoAsync();

        var r = await Asignador().HandleAsync(new(viaje.Id, await VehiculoAsync(), await ConductorAsync()), default);

        Assert.True(r.Exito);
        Assert.Equal("Asignado", r.Valor!.Estado);
    }

    [Fact]
    public async Task Soat_vencido_devuelve_error_de_validacion()
    {
        var viaje = await ViajePlaneadoAsync();

        var r = await Asignador().HandleAsync(new(viaje.Id, await VehiculoAsync(soat: Hoy.AddDays(-1)), await ConductorAsync()), default);

        Assert.Equal(TipoError.Validacion, r.TipoError);
        Assert.Equal("DOCUMENTO_VEHICULO_VENCIDO", Assert.Single(r.Errores).Codigo);
    }

    [Fact]
    public async Task Capacidad_insuficiente_devuelve_error_de_validacion()
    {
        var viaje = await ViajePlaneadoAsync(pesoKg: 12_000);

        var r = await Asignador().HandleAsync(new(viaje.Id, await VehiculoAsync(capacidadKg: 10_000), await ConductorAsync()), default);

        Assert.Equal(TipoError.Validacion, r.TipoError);
        Assert.Equal("CAPACIDAD_INSUFICIENTE", Assert.Single(r.Errores).Codigo);
    }

    [Fact]
    public async Task Vehiculo_en_otro_viaje_activo_devuelve_conflicto()
    {
        var vehiculoId = await VehiculoAsync();
        var primero = await ViajePlaneadoAsync();
        await Asignador().HandleAsync(new(primero.Id, vehiculoId, await ConductorAsync("1037000111")), default);
        var segundo = await ViajePlaneadoAsync();

        var r = await Asignador().HandleAsync(new(segundo.Id, vehiculoId, await ConductorAsync("1037000222")), default);

        Assert.Equal(TipoError.Conflicto, r.TipoError);
        Assert.Equal("VEHICULO_NO_DISPONIBLE", Assert.Single(r.Errores).Codigo);
    }

    [Fact]
    public async Task Viaje_inexistente_devuelve_no_encontrado()
    {
        var r = await Asignador().HandleAsync(new(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()), default);

        Assert.Equal(TipoError.NoEncontrado, r.TipoError);
    }

    [Fact]
    public async Task Orden_de_cliente_inexistente_devuelve_no_encontrado()
    {
        var r = await new CrearOrdenHandler(_clientes, _ordenes).HandleAsync(
            new(Guid.NewGuid(), "Medellín", "Cali", "Textiles", 1_000, 5m, Hoy), default);

        Assert.Equal(TipoError.NoEncontrado, r.TipoError);
    }

    [Fact]
    public async Task Una_orden_no_genera_dos_viajes()
    {
        var viaje = await ViajePlaneadoAsync();

        var r = await new CrearViajeHandler(_ordenes, _viajes).HandleAsync(new(viaje.OrdenId), default);

        Assert.Equal(TipoError.Conflicto, r.TipoError);
    }

    [Fact]
    public async Task Placa_duplicada_devuelve_conflicto()
    {
        await VehiculoAsync("TAC123");

        var r = await new RegistrarVehiculoHandler(_vehiculos).HandleAsync(
            new("tac-123", "Camión", 5_000, Vinculacion.Afiliado, Hoy, Hoy), default);

        Assert.Equal("PLACA_DUPLICADA", Assert.Single(r.Errores).Codigo);
    }
}
