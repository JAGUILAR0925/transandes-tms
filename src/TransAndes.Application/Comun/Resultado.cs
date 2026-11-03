namespace TransAndes.Application.Comun;

/// <summary>Categoría del error de un caso de uso; la API la traduce a un código HTTP.</summary>
public enum TipoError
{
    /// <summary>422 — datos o reglas de negocio incumplidas.</summary>
    Validacion,

    /// <summary>404 — el recurso referenciado no existe.</summary>
    NoEncontrado,

    /// <summary>409 — conflicto con el estado actual (p. ej. recurso ocupado).</summary>
    Conflicto,
}

/// <summary>Detalle de un error de negocio.</summary>
public sealed record ErrorDetalle(string Codigo, string Mensaje);

/// <summary>Resultado de un caso de uso: valor o lista de errores de un mismo tipo.</summary>
public sealed class Resultado<T>
{
    internal Resultado(T? valor, TipoError? tipo, IReadOnlyList<ErrorDetalle> errores)
    {
        Valor = valor;
        TipoError = tipo;
        Errores = errores;
    }

    public T? Valor { get; }
    public TipoError? TipoError { get; }
    public IReadOnlyList<ErrorDetalle> Errores { get; }
    public bool Exito => TipoError is null;
}

/// <summary>Fábricas de <see cref="Resultado{T}"/>.</summary>
public static class Resultado
{
    public static Resultado<T> Ok<T>(T valor) => new(valor, null, []);

    public static Resultado<T> Falla<T>(TipoError tipo, params ErrorDetalle[] errores) => new(default, tipo, errores);

    public static Resultado<T> Falla<T>(TipoError tipo, IEnumerable<ErrorDetalle> errores) => new(default, tipo, errores.ToList());
}
