var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.MapGet("/health", () => Results.Ok(new { status = "ok", service = "TransAndes.Api" }));

app.Run();

/// <summary>Expuesto para pruebas de integración con WebApplicationFactory.</summary>
public partial class Program;
