using TransAndes.Domain.Comun;

namespace TransAndes.Domain.Clientes;

/// <summary>Cliente generador de carga (data-model.md). Raíz de agregado.</summary>
public sealed class Cliente
{
    public Guid Id { get; }
    public string Nit { get; }
    public string RazonSocial { get; }

    public Cliente(Guid id, string nit, string razonSocial)
    {
        if (string.IsNullOrWhiteSpace(nit))
        {
            throw new DomainException("El NIT del cliente es obligatorio.");
        }

        if (string.IsNullOrWhiteSpace(razonSocial))
        {
            throw new DomainException("La razón social del cliente es obligatoria.");
        }

        Id = id;
        Nit = nit.Trim();
        RazonSocial = razonSocial.Trim();
    }
}
