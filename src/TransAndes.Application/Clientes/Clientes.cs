using TransAndes.Application.Abstracciones;
using TransAndes.Application.Comun;
using TransAndes.Domain.Clientes;
using TransAndes.Domain.Comun;

namespace TransAndes.Application.Clientes;

public sealed record RegistrarClienteCommand(string Nit, string RazonSocial);

public sealed class RegistrarClienteHandler(IClienteRepository clientes)
{
    public async Task<Resultado<Guid>> HandleAsync(RegistrarClienteCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);
        Cliente cliente;
        try
        {
            cliente = new Cliente(Guid.NewGuid(), cmd.Nit, cmd.RazonSocial);
        }
        catch (DomainException ex)
        {
            return Resultado.Falla<Guid>(TipoError.Validacion, new ErrorDetalle("CLIENTE_INVALIDO", ex.Message));
        }

        if (await clientes.ExisteNitAsync(cliente.Nit, ct).ConfigureAwait(false))
        {
            return Resultado.Falla<Guid>(TipoError.Conflicto, new ErrorDetalle("NIT_DUPLICADO", $"Ya existe un cliente con NIT {cliente.Nit}."));
        }

        await clientes.AgregarAsync(cliente, ct).ConfigureAwait(false);
        return Resultado.Ok(cliente.Id);
    }
}
