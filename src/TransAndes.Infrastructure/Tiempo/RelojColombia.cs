using TransAndes.Application.Abstracciones;

namespace TransAndes.Infrastructure.Tiempo;

/// <summary>
/// Fecha de operación en hora de Colombia (UTC−5, sin horario de verano).
/// Se usa un desplazamiento fijo para no depender de la base de zonas horarias del contenedor.
/// </summary>
public sealed class RelojColombia(TimeProvider tiempo) : IReloj
{
    private static readonly TimeSpan Desfase = TimeSpan.FromHours(-5);

    public DateOnly Hoy => DateOnly.FromDateTime(tiempo.GetUtcNow().ToOffset(Desfase).DateTime);
}
