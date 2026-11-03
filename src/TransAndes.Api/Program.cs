using System.Text.Json.Serialization;
using TransAndes.Api.Configuracion;
using TransAndes.Api.Endpoints;
using TransAndes.Api.Seguridad;

var builder = WebApplication.CreateBuilder(args);

// Enums solo como texto: un número fuera de rango (p. ej. "vinculacion": 99) responde 400 (hallazgo #3).
builder.Services.ConfigureHttpJsonOptions(o =>
    o.SerializerOptions.Converters.Add(new JsonStringEnumConverter(namingPolicy: null, allowIntegerValues: false)));
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<SolicitudInvalidaHandler>();
builder.Services.AddTransAndesTms();

var app = builder.Build();

app.UseCabecerasSeguridad();
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseExceptionHandler();
app.UseStatusCodePages();

app.MapGet("/health", () => Results.Ok(new { status = "ok", service = "TransAndes.Api" }));
app.MapDespacho();

app.Run();

/// <summary>Expuesto para pruebas de integración con WebApplicationFactory.</summary>
public partial class Program;
