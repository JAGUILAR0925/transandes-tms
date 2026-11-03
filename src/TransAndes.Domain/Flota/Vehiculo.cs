using System.Text.RegularExpressions;
using TransAndes.Domain.Comun;

namespace TransAndes.Domain.Flota;

/// <summary>Vehículo de carga (data-model.md). Raíz de agregado.</summary>
public sealed partial class Vehiculo
{
    public Guid Id { get; }
    public string Placa { get; }
    public string Tipo { get; }
    public int CapacidadKg { get; }
    public Vinculacion Vinculacion { get; }
    public DateOnly VenceSoat { get; private set; }
    public DateOnly VenceRtm { get; private set; }

    public Vehiculo(Guid id, string placa, string tipo, int capacidadKg, Vinculacion vinculacion, DateOnly venceSoat, DateOnly venceRtm)
    {
        var placaNormalizada = (placa ?? string.Empty).Trim().ToUpperInvariant().Replace(" ", string.Empty, StringComparison.Ordinal).Replace("-", string.Empty, StringComparison.Ordinal);
        if (!FormatoPlaca().IsMatch(placaNormalizada))
        {
            throw new DomainException("La placa debe tener el formato AAA123.");
        }

        if (!Enum.IsDefined(vinculacion))
        {
            throw new DomainException($"Vinculación no válida: {(int)vinculacion}.");
        }

        if (capacidadKg <= 0)
        {
            throw new DomainException("La capacidad del vehículo debe ser mayor que cero.");
        }

        Id = id;
        Placa = placaNormalizada;
        Tipo = Texto.Requerido(tipo, "El tipo de vehículo", Limites.TipoVehiculo);
        CapacidadKg = capacidadKg;
        Vinculacion = vinculacion;
        VenceSoat = venceSoat;
        VenceRtm = venceRtm;
    }

    /// <summary>SOAT y revisión técnico-mecánica vigentes en la fecha dada (vigente hasta el día de vencimiento inclusive).</summary>
    public bool DocumentosVigentes(DateOnly hoy) => VenceSoat >= hoy && VenceRtm >= hoy;

    /// <summary>Lista de documentos vencidos, para mensajes claros al coordinador.</summary>
    public IReadOnlyList<string> DocumentosVencidos(DateOnly hoy)
    {
        var vencidos = new List<string>();
        if (VenceSoat < hoy)
        {
            vencidos.Add($"SOAT (venció {VenceSoat:yyyy-MM-dd})");
        }

        if (VenceRtm < hoy)
        {
            vencidos.Add($"Revisión técnico-mecánica (venció {VenceRtm:yyyy-MM-dd})");
        }

        return vencidos;
    }

    public void ActualizarDocumentos(DateOnly venceSoat, DateOnly venceRtm)
    {
        VenceSoat = venceSoat;
        VenceRtm = venceRtm;
    }

    [GeneratedRegex("^[A-Z]{3}[0-9]{3}$")]
    private static partial Regex FormatoPlaca();
}
