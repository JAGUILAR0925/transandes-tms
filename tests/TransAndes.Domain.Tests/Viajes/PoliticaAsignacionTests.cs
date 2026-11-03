using TransAndes.Domain.Flota;
using TransAndes.Domain.Ordenes;
using TransAndes.Domain.Viajes;

namespace TransAndes.Domain.Tests.Viajes;

/// <summary>T015 — validaciones de asignación (FR-003, US1 escenarios 2-4).</summary>
public class PoliticaAsignacionTests
{
    private static readonly DateOnly Hoy = new(2027, 1, 15);

    private static OrdenDeCargue Orden(int pesoKg = 8_000) =>
        new(Guid.NewGuid(), Guid.NewGuid(), "Medellín", "Barranquilla", "Alimentos", pesoKg, 30m, Hoy.AddDays(2));

    private static Vehiculo Vehiculo(int capacidadKg = 10_000, DateOnly? soat = null, DateOnly? rtm = null) =>
        new(Guid.NewGuid(), "TAC123", "Camión sencillo", capacidadKg, Vinculacion.Propio, soat ?? Hoy.AddMonths(6), rtm ?? Hoy.AddMonths(6));

    private static Conductor Conductor(DateOnly? licencia = null) =>
        new(Guid.NewGuid(), "1037000111", "Carlos Pérez", "3001234567", "C2", licencia ?? Hoy.AddYears(1));

    private static (Viaje Viaje, OrdenDeCargue Orden) ViajeConOrden(int pesoKg = 8_000)
    {
        var orden = Orden(pesoKg);
        return (new Viaje(Guid.NewGuid(), "V-0001", orden.Id), orden);
    }

    [Fact]
    public void Recursos_validos_no_generan_errores()
    {
        var (viaje, orden) = ViajeConOrden();

        var errores = PoliticaAsignacion.Validar(viaje, orden, Vehiculo(), Conductor(), Hoy, false, false);

        Assert.Empty(errores);
    }

    [Fact]
    public void Soat_vencido_impide_asignar_y_nombra_el_documento()
    {
        var (viaje, orden) = ViajeConOrden();

        var errores = PoliticaAsignacion.Validar(viaje, orden, Vehiculo(soat: Hoy.AddDays(-1)), Conductor(), Hoy, false, false);

        var error = Assert.Single(errores);
        Assert.Equal("DOCUMENTO_VEHICULO_VENCIDO", error.Codigo);
        Assert.Contains("SOAT", error.Mensaje, StringComparison.Ordinal);
        Assert.Equal(TipoErrorAsignacion.Validacion, error.Tipo);
    }

    [Fact]
    public void Documento_que_vence_hoy_sigue_vigente()
    {
        var (viaje, orden) = ViajeConOrden();

        var errores = PoliticaAsignacion.Validar(viaje, orden, Vehiculo(soat: Hoy, rtm: Hoy), Conductor(licencia: Hoy), Hoy, false, false);

        Assert.Empty(errores);
    }

    [Fact]
    public void Capacidad_insuficiente_indica_los_kilos_faltantes()
    {
        var (viaje, orden) = ViajeConOrden(pesoKg: 12_000);

        var errores = PoliticaAsignacion.Validar(viaje, orden, Vehiculo(capacidadKg: 10_000), Conductor(), Hoy, false, false);

        var error = Assert.Single(errores);
        Assert.Equal("CAPACIDAD_INSUFICIENTE", error.Codigo);
        Assert.Contains("2000 kg", error.Mensaje, StringComparison.Ordinal);
    }

    [Fact]
    public void Licencia_vencida_impide_asignar()
    {
        var (viaje, orden) = ViajeConOrden();

        var errores = PoliticaAsignacion.Validar(viaje, orden, Vehiculo(), Conductor(licencia: Hoy.AddDays(-30)), Hoy, false, false);

        Assert.Equal("LICENCIA_VENCIDA", Assert.Single(errores).Codigo);
    }

    [Fact]
    public void Recursos_ocupados_son_error_de_disponibilidad()
    {
        var (viaje, orden) = ViajeConOrden();

        var errores = PoliticaAsignacion.Validar(viaje, orden, Vehiculo(), Conductor(), Hoy, true, true);

        Assert.Equal(2, errores.Count);
        Assert.All(errores, e => Assert.Equal(TipoErrorAsignacion.NoDisponible, e.Tipo));
    }

    [Fact]
    public void Reporta_todos_los_incumplimientos_a_la_vez()
    {
        var (viaje, orden) = ViajeConOrden(pesoKg: 15_000);

        var errores = PoliticaAsignacion.Validar(
            viaje, orden, Vehiculo(capacidadKg: 10_000, soat: Hoy.AddDays(-1), rtm: Hoy.AddDays(-1)), Conductor(licencia: Hoy.AddDays(-1)), Hoy, true, false);

        Assert.Equal(5, errores.Count);
    }

    [Fact]
    public void Viaje_ya_asignado_no_se_puede_volver_a_asignar()
    {
        var (viaje, orden) = ViajeConOrden();
        viaje.Asignar(Guid.NewGuid(), Guid.NewGuid());

        var errores = PoliticaAsignacion.Validar(viaje, orden, Vehiculo(), Conductor(), Hoy, false, false);

        Assert.Equal("ESTADO_INVALIDO", Assert.Single(errores).Codigo);
    }

    [Fact]
    public void Orden_de_otro_viaje_es_rechazada()
    {
        var (viaje, _) = ViajeConOrden();

        var errores = PoliticaAsignacion.Validar(viaje, Orden(), Vehiculo(), Conductor(), Hoy, false, false);

        Assert.Equal("ORDEN_NO_CORRESPONDE", Assert.Single(errores).Codigo);
    }
}
