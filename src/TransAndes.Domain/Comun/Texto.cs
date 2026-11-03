namespace TransAndes.Domain.Comun;

/// <summary>
/// Validación de textos de entrada: obligatoriedad y longitud máxima.
/// Los límites coinciden con las columnas que tendrá el esquema SQL (T010) y evitan abuso con cargas enormes.
/// </summary>
public static class Texto
{
    public static string Requerido(string? valor, string campo, int maximo)
    {
        if (string.IsNullOrWhiteSpace(valor))
        {
            throw new DomainException($"{campo} es obligatorio.");
        }

        return Acotar(valor.Trim(), campo, maximo);
    }

    public static string Opcional(string? valor, string campo, int maximo) =>
        string.IsNullOrWhiteSpace(valor) ? string.Empty : Acotar(valor.Trim(), campo, maximo);

    private static string Acotar(string valor, string campo, int maximo) =>
        valor.Length <= maximo
            ? valor
            : throw new DomainException($"{campo} admite máximo {maximo} caracteres (recibidos {valor.Length}).");
}

/// <summary>Longitudes máximas compartidas por dominio y persistencia.</summary>
public static class Limites
{
    public const int Nit = 20;
    public const int RazonSocial = 200;
    public const int TipoVehiculo = 60;
    public const int NombrePersona = 150;
    public const int Celular = 20;
    public const int CategoriaLicencia = 5;
    public const int Lugar = 120;
    public const int TipoMercancia = 100;
}
