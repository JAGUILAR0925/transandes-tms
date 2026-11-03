using TransAndes.Domain.Comun;
using TransAndes.Domain.Ordenes;

namespace TransAndes.Domain.Tests.Ordenes;

public class OrdenDeCargueTests
{
    private static readonly DateOnly Fecha = new(2027, 1, 20);

    [Fact]
    public void Orden_valida_se_crea()
    {
        var orden = new OrdenDeCargue(Guid.NewGuid(), Guid.NewGuid(), " Medellín ", "Cali", "Textiles", 5_000, 20m, Fecha);

        Assert.Equal("Medellín", orden.Origen);
        Assert.Equal(5_000, orden.PesoKg);
    }

    [Fact]
    public void Origen_y_destino_iguales_son_rechazados()
    {
        Assert.Throws<DomainException>(() => new OrdenDeCargue(Guid.NewGuid(), Guid.NewGuid(), "Cali", "cali", "Textiles", 5_000, 20m, Fecha));
    }

    [Fact]
    public void Peso_debe_ser_positivo()
    {
        Assert.Throws<DomainException>(() => new OrdenDeCargue(Guid.NewGuid(), Guid.NewGuid(), "Cali", "Pasto", "Textiles", 0, 20m, Fecha));
    }
}
