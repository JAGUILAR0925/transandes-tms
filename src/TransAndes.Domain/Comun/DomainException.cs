namespace TransAndes.Domain.Comun;

/// <summary>Violación de una regla de negocio del dominio.</summary>
public sealed class DomainException(string message) : Exception(message);
