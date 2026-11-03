using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using TransAndes.Application.Abstracciones;

namespace TransAndes.Api.IntegrationTests.Despacho;

/// <summary>T016 — flujo de US1 contra la API real en memoria (contracts/api.yaml).</summary>
public sealed class DespachoApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private static readonly DateOnly Hoy = new(2027, 1, 15);
    private readonly HttpClient _http;

    private sealed class RelojFijo : IReloj
    {
        public DateOnly Hoy => DespachoApiTests.Hoy;
    }

    public DespachoApiTests(WebApplicationFactory<Program> factory)
    {
        ArgumentNullException.ThrowIfNull(factory);
        _http = factory
            .WithWebHostBuilder(b => b.ConfigureTestServices(s => s.AddSingleton<IReloj, RelojFijo>()))
            .CreateClient();
    }

    private async Task<Guid> CrearAsync(string ruta, object cuerpo)
    {
        var resp = await _http.PostAsJsonAsync(new Uri(ruta, UriKind.Relative), cuerpo);
        Assert.Equal(HttpStatusCode.Created, resp.StatusCode);
        using var json = JsonDocument.Parse(await resp.Content.ReadAsStringAsync());
        return json.RootElement.GetProperty("id").GetGuid();
    }

    private static string Unico() => Guid.NewGuid().ToString("N")[..6];

    private Task<Guid> ClienteAsync() => CrearAsync("/v1/clientes", new { nit = $"900{Unico()}-1", razonSocial = "Cliente de prueba" });

    private Task<Guid> VehiculoAsync(int capacidadKg = 10_000, DateOnly? soat = null)
    {
        var placa = "T" + string.Concat(Unico().Take(2).Select(c => (char)('A' + (c % 26)))) + Random.Shared.Next(100, 999);
        return CrearAsync("/v1/vehiculos", new
        {
            placa,
            tipo = "Camión sencillo",
            capacidadKg,
            vinculacion = "Propio",
            venceSoat = soat ?? Hoy.AddMonths(6),
            venceRtm = Hoy.AddMonths(6),
        });
    }

    private Task<Guid> ConductorAsync() => CrearAsync("/v1/conductores", new
    {
        cedula = Random.Shared.NextInt64(1_000_000_000, 9_999_999_999).ToString(System.Globalization.CultureInfo.InvariantCulture),
        nombre = "Conductor de prueba",
        celular = "3000000000",
        licenciaCategoria = "C2",
        venceLicencia = Hoy.AddYears(1),
    });

    private async Task<Guid> ViajeAsync(int pesoKg = 8_000)
    {
        var ordenId = await CrearAsync("/v1/ordenes", new
        {
            clienteId = await ClienteAsync(),
            origen = "Medellín",
            destino = "Barranquilla",
            tipoMercancia = "Alimentos",
            pesoKg,
            volumenM3 = 30,
            fechaRequerida = Hoy.AddDays(2),
        });
        return await CrearAsync("/v1/viajes", new { ordenId });
    }

    private Task<HttpResponseMessage> AsignarAsync(Guid viajeId, Guid vehiculoId, Guid conductorId) =>
        _http.PutAsJsonAsync(new Uri($"/v1/viajes/{viajeId}/asignacion", UriKind.Relative), new { vehiculoId, conductorId });

    private static async Task<string> CodigoErrorAsync(HttpResponseMessage resp)
    {
        using var json = JsonDocument.Parse(await resp.Content.ReadAsStringAsync());
        return json.RootElement.GetProperty("errores")[0].GetProperty("codigo").GetString()!;
    }

    [Fact]
    public async Task Health_responde_ok()
    {
        var resp = await _http.GetAsync(new Uri("/health", UriKind.Relative));

        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
    }

    [Fact]
    public async Task Flujo_completo_deja_el_viaje_asignado()
    {
        var viajeId = await ViajeAsync();

        var resp = await AsignarAsync(viajeId, await VehiculoAsync(), await ConductorAsync());

        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        using var json = JsonDocument.Parse(await resp.Content.ReadAsStringAsync());
        Assert.Equal("Asignado", json.RootElement.GetProperty("estado").GetString());
    }

    [Fact]
    public async Task Soat_vencido_responde_422()
    {
        var viajeId = await ViajeAsync();

        var resp = await AsignarAsync(viajeId, await VehiculoAsync(soat: Hoy.AddDays(-1)), await ConductorAsync());

        Assert.Equal(HttpStatusCode.UnprocessableEntity, resp.StatusCode);
        Assert.Equal("DOCUMENTO_VEHICULO_VENCIDO", await CodigoErrorAsync(resp));
    }

    [Fact]
    public async Task Capacidad_insuficiente_responde_422()
    {
        var viajeId = await ViajeAsync(pesoKg: 12_000);

        var resp = await AsignarAsync(viajeId, await VehiculoAsync(capacidadKg: 10_000), await ConductorAsync());

        Assert.Equal(HttpStatusCode.UnprocessableEntity, resp.StatusCode);
        Assert.Equal("CAPACIDAD_INSUFICIENTE", await CodigoErrorAsync(resp));
    }

    [Fact]
    public async Task Vehiculo_ocupado_responde_409()
    {
        var vehiculoId = await VehiculoAsync();
        Assert.Equal(HttpStatusCode.OK, (await AsignarAsync(await ViajeAsync(), vehiculoId, await ConductorAsync())).StatusCode);

        var resp = await AsignarAsync(await ViajeAsync(), vehiculoId, await ConductorAsync());

        Assert.Equal(HttpStatusCode.Conflict, resp.StatusCode);
        Assert.Equal("VEHICULO_NO_DISPONIBLE", await CodigoErrorAsync(resp));
    }

    [Fact]
    public async Task Viaje_inexistente_responde_404()
    {
        var resp = await AsignarAsync(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

        Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
    }
}
