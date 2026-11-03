using System.Text.RegularExpressions;
using TransAndes.Domain.Comun;

namespace TransAndes.Domain.Flota;

/// <summary>Conductor (data-model.md). Raíz de agregado. Cédula y celular son datos personales (Ley 1581).</summary>
public sealed partial class Conductor
{
    public Guid Id { get; }
    public string Cedula { get; }
    public string Nombre { get; }
    public string Celular { get; }
    public string LicenciaCategoria { get; }
    public DateOnly VenceLicencia { get; private set; }

    public Conductor(Guid id, string cedula, string nombre, string celular, string licenciaCategoria, DateOnly venceLicencia)
    {
        var cedulaLimpia = (cedula ?? string.Empty).Trim().Replace(".", string.Empty, StringComparison.Ordinal);
        if (!FormatoCedula().IsMatch(cedulaLimpia))
        {
            throw new DomainException("La cédula debe tener entre 6 y 10 dígitos.");
        }

        if (string.IsNullOrWhiteSpace(nombre))
        {
            throw new DomainException("El nombre del conductor es obligatorio.");
        }

        if (string.IsNullOrWhiteSpace(licenciaCategoria))
        {
            throw new DomainException("La categoría de licencia es obligatoria.");
        }

        Id = id;
        Cedula = cedulaLimpia;
        Nombre = nombre.Trim();
        Celular = (celular ?? string.Empty).Trim();
        LicenciaCategoria = licenciaCategoria.Trim().ToUpperInvariant();
        VenceLicencia = venceLicencia;
    }

    /// <summary>Licencia vigente en la fecha dada (vigente hasta el día de vencimiento inclusive).</summary>
    public bool LicenciaVigente(DateOnly hoy) => VenceLicencia >= hoy;

    public void RenovarLicencia(DateOnly venceLicencia) => VenceLicencia = venceLicencia;

    [GeneratedRegex("^[0-9]{6,10}$")]
    private static partial Regex FormatoCedula();
}
