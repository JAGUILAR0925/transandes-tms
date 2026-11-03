namespace TransAndes.Application.Abstracciones;

/// <summary>Fecha de operación (hora de Colombia). Abstraída para pruebas deterministas.</summary>
public interface IReloj
{
    DateOnly Hoy { get; }
}
