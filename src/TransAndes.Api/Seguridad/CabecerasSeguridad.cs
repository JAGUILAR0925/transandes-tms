namespace TransAndes.Api.Seguridad;

/// <summary>
/// Cabeceras de seguridad para una API JSON (OWASP A02:2025, hallazgo #6).
/// HSTS se activa fuera de desarrollo; el redireccionamiento a HTTPS lo hace Front Door (ver arquitectura).
/// </summary>
public static class CabecerasSeguridad
{
    public static IApplicationBuilder UseCabecerasSeguridad(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);
        return app.Use(async (contexto, siguiente) =>
        {
            var h = contexto.Response.Headers;
            h.XContentTypeOptions = "nosniff";
            h.XFrameOptions = "DENY";
            h["Referrer-Policy"] = "no-referrer";
            h.ContentSecurityPolicy = "default-src 'none'; frame-ancestors 'none'";
            await siguiente(contexto).ConfigureAwait(false);
        });
    }
}
