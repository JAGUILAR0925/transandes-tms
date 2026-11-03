using TransAndes.Domain.Comun;
using TransAndes.Domain.Flota;

namespace TransAndes.Domain.Tests.Flota;

public class FlotaTests
{
    private static readonly DateOnly Hoy = new(2027, 1, 15);

    [Fact]
    public void Placa_se_normaliza_a_mayusculas_sin_separadores()
    {
        var vehiculo = new Vehiculo(Guid.NewGuid(), " tac-123 ", "Tractocamión", 34_000, Vinculacion.Afiliado, Hoy, Hoy);

        Assert.Equal("TAC123", vehiculo.Placa);
    }

    [Fact]
    public void Placa_con_formato_invalido_es_rechazada()
    {
        Assert.Throws<DomainException>(() => new Vehiculo(Guid.NewGuid(), "12ABC", "Camión", 10_000, Vinculacion.Propio, Hoy, Hoy));
    }

    [Fact]
    public void Capacidad_debe_ser_positiva()
    {
        Assert.Throws<DomainException>(() => new Vehiculo(Guid.NewGuid(), "TAC123", "Camión", 0, Vinculacion.Propio, Hoy, Hoy));
    }

    [Fact]
    public void Documentos_vencidos_se_listan_por_nombre()
    {
        var vehiculo = new Vehiculo(Guid.NewGuid(), "TAC123", "Camión", 10_000, Vinculacion.Propio, Hoy.AddDays(-1), Hoy.AddDays(-2));

        Assert.False(vehiculo.DocumentosVigentes(Hoy));
        Assert.Equal(2, vehiculo.DocumentosVencidos(Hoy).Count);
    }

    [Fact]
    public void Cedula_acepta_puntos_y_los_elimina()
    {
        var conductor = new Conductor(Guid.NewGuid(), "1.037.000.111", "Ana Gómez", "3001112233", "c2", Hoy);

        Assert.Equal("1037000111", conductor.Cedula);
        Assert.Equal("C2", conductor.LicenciaCategoria);
    }

    [Fact]
    public void Cedula_invalida_es_rechazada()
    {
        Assert.Throws<DomainException>(() => new Conductor(Guid.NewGuid(), "12A", "Ana", "300", "C2", Hoy));
    }
}
