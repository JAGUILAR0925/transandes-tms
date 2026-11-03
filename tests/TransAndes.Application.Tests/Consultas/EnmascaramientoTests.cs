using TransAndes.Application.Consultas;

namespace TransAndes.Application.Tests.Consultas;

public class EnmascaramientoTests
{
    [Fact]
    public void Cedula_deja_visibles_solo_los_ultimos_cuatro_digitos()
    {
        Assert.Equal("******0111", ConsultasHandler.Enmascarar("1037000111"));
    }

    [Fact]
    public void Cedula_corta_se_oculta_completa()
    {
        Assert.Equal("****", ConsultasHandler.Enmascarar("1234"));
    }
}
