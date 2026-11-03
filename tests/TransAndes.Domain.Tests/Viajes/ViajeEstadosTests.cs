using TransAndes.Domain.Comun;
using TransAndes.Domain.Viajes;

namespace TransAndes.Domain.Tests.Viajes;

/// <summary>T007 — máquina de estados del viaje (docs/architecture/05-estados.md §5.1).</summary>
public class ViajeEstadosTests
{
    private static Viaje NuevoViaje() => new(Guid.NewGuid(), "V-0001", Guid.NewGuid());

    private static Viaje ViajeDespachado()
    {
        var viaje = NuevoViaje();
        viaje.Asignar(Guid.NewGuid(), Guid.NewGuid());
        viaje.MarcarDespachado("MAN-123");
        return viaje;
    }

    [Fact]
    public void Nuevo_viaje_inicia_planeado()
    {
        Assert.Equal(EstadoViaje.Planeado, NuevoViaje().Estado);
    }

    [Fact]
    public void Flujo_feliz_llega_a_entregado()
    {
        var viaje = ViajeDespachado();
        viaje.IniciarRuta();
        viaje.Entregar(pruebaDeEntregaCompleta: true);

        Assert.Equal(EstadoViaje.Entregado, viaje.Estado);
        Assert.Equal("MAN-123", viaje.NumeroManifiesto);
    }

    [Fact]
    public void No_despacha_sin_numero_de_manifiesto()
    {
        var viaje = NuevoViaje();
        viaje.Asignar(Guid.NewGuid(), Guid.NewGuid());

        Assert.Throws<DomainException>(() => viaje.MarcarDespachado(" "));
    }

    [Fact]
    public void No_despacha_si_no_esta_asignado()
    {
        Assert.Throws<DomainException>(() => NuevoViaje().MarcarDespachado("MAN-1"));
    }

    [Fact]
    public void Rechazo_rndc_permite_corregir_y_reenviar()
    {
        var viaje = NuevoViaje();
        viaje.Asignar(Guid.NewGuid(), Guid.NewGuid());
        viaje.MarcarRechazadoRndc("Placa inválida");
        viaje.PrepararReenvio();

        Assert.Equal(EstadoViaje.Asignado, viaje.Estado);
    }

    [Fact]
    public void No_entrega_sin_prueba_completa()
    {
        var viaje = ViajeDespachado();
        viaje.IniciarRuta();

        Assert.Throws<DomainException>(() => viaje.Entregar(pruebaDeEntregaCompleta: false));
    }

    [Fact]
    public void Anular_despachado_exige_anulacion_rndc()
    {
        var viaje = ViajeDespachado();

        Assert.Throws<DomainException>(() => viaje.Anular());
        viaje.Anular(anulacionRndcConfirmada: true);
        Assert.Equal(EstadoViaje.Anulado, viaje.Estado);
    }

    [Fact]
    public void No_se_anula_un_viaje_entregado()
    {
        var viaje = ViajeDespachado();
        viaje.IniciarRuta();
        viaje.Entregar(pruebaDeEntregaCompleta: true);

        Assert.Throws<DomainException>(() => viaje.Anular(anulacionRndcConfirmada: true));
    }

    [Fact]
    public void Desasignar_libera_recursos()
    {
        var viaje = NuevoViaje();
        viaje.Asignar(Guid.NewGuid(), Guid.NewGuid());
        viaje.Desasignar();

        Assert.Equal(EstadoViaje.Planeado, viaje.Estado);
        Assert.Null(viaje.VehiculoId);
        Assert.Null(viaje.ConductorId);
    }
}
