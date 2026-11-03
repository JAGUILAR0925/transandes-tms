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
        Id = id;
        Nit = Texto.Requerido(nit, "El NIT del cliente", Limites.Nit);
        RazonSocial = Texto.Requerido(razonSocial, "La razón social del cliente", Limites.RazonSocial);
    }
}
