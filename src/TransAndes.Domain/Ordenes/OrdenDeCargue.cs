using TransAndes.Domain.Comun;

namespace TransAndes.Domain.Ordenes;

/// <summary>Solicitud de transporte de un cliente (FR-001). Raíz de agregado.</summary>
public sealed class OrdenDeCargue
{
    public Guid Id { get; }
    public Guid ClienteId { get; }
    public string Origen { get; }
    public string Destino { get; }
    public string TipoMercancia { get; }
    public int PesoKg { get; }
    public decimal VolumenM3 { get; }
    public DateOnly FechaRequerida { get; }

    public OrdenDeCargue(Guid id, Guid clienteId, string origen, string destino, string tipoMercancia, int pesoKg, decimal volumenM3, DateOnly fechaRequerida)
    {
        if (string.IsNullOrWhiteSpace(origen) || string.IsNullOrWhiteSpace(destino))
        {
            throw new DomainException("Origen y destino son obligatorios.");
        }

        if (string.Equals(origen.Trim(), destino.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            throw new DomainException("El origen y el destino deben ser diferentes.");
        }

        if (pesoKg <= 0)
        {
            throw new DomainException("El peso de la carga debe ser mayor que cero.");
        }

        if (volumenM3 < 0)
        {
            throw new DomainException("El volumen no puede ser negativo.");
        }

        Id = id;
        ClienteId = clienteId;
        Origen = origen.Trim();
        Destino = destino.Trim();
        TipoMercancia = (tipoMercancia ?? string.Empty).Trim();
        PesoKg = pesoKg;
        VolumenM3 = volumenM3;
        FechaRequerida = fechaRequerida;
    }
}
