using TransAndes.Domain.Clientes;
using TransAndes.Domain.Comun;
using TransAndes.Domain.Flota;
using TransAndes.Domain.Ordenes;

namespace TransAndes.Domain.Tests.Comun;

/// <summary>Hallazgos #3 y #4 de la revisión del PR #2: enums fuera de rango y textos sin límite.</summary>
public class ValidacionEntradaTests
{
    private static readonly DateOnly Hoy = new(2027, 1, 15);

    [Fact]
    public void Vinculacion_fuera_de_rango_es_rechazada()
    {
        var ex = Assert.Throws<DomainException>(() =>
            new Vehiculo(Guid.NewGuid(), "TAC123", "Camión", 10_000, (Vinculacion)99, Hoy, Hoy));

        Assert.Contains("Vinculación", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Razon_social_por_encima_del_limite_es_rechazada()
    {
        var ex = Assert.Throws<DomainException>(() =>
            new Cliente(Guid.NewGuid(), "900123456-7", new string('A', Limites.RazonSocial + 1)));

        Assert.Contains($"máximo {Limites.RazonSocial}", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Razon_social_en_el_limite_es_aceptada()
    {
        var cliente = new Cliente(Guid.NewGuid(), "900123456-7", new string('A', Limites.RazonSocial));

        Assert.Equal(Limites.RazonSocial, cliente.RazonSocial.Length);
    }

    [Fact]
    public void Nit_vacio_es_rechazado()
    {
        Assert.Throws<DomainException>(() => new Cliente(Guid.NewGuid(), "  ", "Empresa"));
    }

    [Fact]
    public void Nombre_de_conductor_demasiado_largo_es_rechazado()
    {
        Assert.Throws<DomainException>(() =>
            new Conductor(Guid.NewGuid(), "1037000111", new string('N', Limites.NombrePersona + 1), "300", "C2", Hoy));
    }

    [Fact]
    public void Celular_es_opcional_pero_acotado()
    {
        var sinCelular = new Conductor(Guid.NewGuid(), "1037000111", "Ana", null!, "C2", Hoy);
        Assert.Equal(string.Empty, sinCelular.Celular);

        Assert.Throws<DomainException>(() =>
            new Conductor(Guid.NewGuid(), "1037000111", "Ana", new string('3', Limites.Celular + 1), "C2", Hoy));
    }

    [Fact]
    public void Origen_demasiado_largo_es_rechazado()
    {
        Assert.Throws<DomainException>(() =>
            new OrdenDeCargue(Guid.NewGuid(), Guid.NewGuid(), new string('M', Limites.Lugar + 1), "Cali", "Textiles", 1_000, 1m, Hoy));
    }

    [Fact]
    public void Tipo_de_vehiculo_demasiado_largo_es_rechazado()
    {
        Assert.Throws<DomainException>(() =>
            new Vehiculo(Guid.NewGuid(), "TAC123", new string('T', Limites.TipoVehiculo + 1), 10_000, Vinculacion.Propio, Hoy, Hoy));
    }
}
