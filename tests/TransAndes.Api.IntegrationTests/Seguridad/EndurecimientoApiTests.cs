using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace TransAndes.Api.IntegrationTests.Seguridad;

/// <summary>Hallazgos #3-#6 de la revisión del PR #2.</summary>
public sealed class EndurecimientoApiTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _http = factory.CreateClient();

    private static object Vehiculo(object vinculacion) => new
    {
        placa = "QWE" + Random.Shared.Next(100, 999),
        tipo = "Camión",
        capacidadKg = 1_000,
        vinculacion,
        venceSoat = "2030-01-01",
        venceRtm = "2030-01-01",
    };

    [Fact]
    public async Task Enum_numerico_es_rechazado_con_400()
    {
        var resp = await _http.PostAsJsonAsync(new Uri("/v1/vehiculos", UriKind.Relative), Vehiculo(99));

        Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
    }

    [Fact]
    public async Task Json_mal_formado_responde_400_y_no_500()
    {
        using var contenido = new StringContent("{\"nit\":", System.Text.Encoding.UTF8, "application/json");

        var resp = await _http.PostAsync(new Uri("/v1/clientes", UriKind.Relative), contenido);

        Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
    }

    [Fact]
    public async Task Texto_por_encima_del_limite_responde_422()
    {
        var resp = await _http.PostAsJsonAsync(new Uri("/v1/clientes", UriKind.Relative), new { nit = "900555444-1", razonSocial = new string('A', 100_000) });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, resp.StatusCode);
    }

    [Fact]
    public async Task Location_de_cada_post_es_consultable()
    {
        var recursos = new (string Ruta, object Cuerpo)[]
        {
            ("/v1/clientes", new { nit = "900" + Random.Shared.Next(100_000, 999_999) + "-2", razonSocial = "Cliente Location" }),
            ("/v1/vehiculos", Vehiculo("Propio")),
            ("/v1/conductores", new { cedula = Random.Shared.Next(10_000_000, 99_999_999).ToString(System.Globalization.CultureInfo.InvariantCulture), nombre = "Ana", celular = "3001112233", licenciaCategoria = "C2", venceLicencia = "2030-01-01" }),
        };

        foreach (var (ruta, cuerpo) in recursos)
        {
            var creado = await _http.PostAsJsonAsync(new Uri(ruta, UriKind.Relative), cuerpo);
            Assert.Equal(HttpStatusCode.Created, creado.StatusCode);

            var consulta = await _http.GetAsync(creado.Headers.Location);
            Assert.Equal(HttpStatusCode.OK, consulta.StatusCode);
        }
    }

    [Fact]
    public async Task Conductor_no_expone_cedula_completa_ni_celular()
    {
        var creado = await _http.PostAsJsonAsync(new Uri("/v1/conductores", UriKind.Relative),
            new { cedula = "1037009876", nombre = "Luis", celular = "3009998877", licenciaCategoria = "C2", venceLicencia = "2030-01-01" });
        var cuerpo = await (await _http.GetAsync(creado.Headers.Location)).Content.ReadAsStringAsync();

        using var json = JsonDocument.Parse(cuerpo);
        Assert.Equal("******9876", json.RootElement.GetProperty("cedulaEnmascarada").GetString());
        Assert.False(cuerpo.Contains("1037009876", StringComparison.Ordinal));
        Assert.False(cuerpo.Contains("3009998877", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Respuestas_incluyen_cabeceras_de_seguridad()
    {
        var resp = await _http.GetAsync(new Uri("/health", UriKind.Relative));

        Assert.Equal("nosniff", resp.Headers.GetValues("X-Content-Type-Options").Single());
        Assert.Equal("DENY", resp.Headers.GetValues("X-Frame-Options").Single());
        Assert.True(resp.Headers.Contains("Content-Security-Policy"));
    }

    [Fact]
    public async Task Recurso_inexistente_responde_404()
    {
        var resp = await _http.GetAsync(new Uri($"/v1/ordenes/{Guid.NewGuid()}", UriKind.Relative));

        Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
    }
}
