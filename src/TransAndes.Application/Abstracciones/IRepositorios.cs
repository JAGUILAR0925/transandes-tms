using TransAndes.Domain.Clientes;
using TransAndes.Domain.Flota;
using TransAndes.Domain.Ordenes;
using TransAndes.Domain.Viajes;

namespace TransAndes.Application.Abstracciones;

public interface IClienteRepository
{
    Task<Cliente?> ObtenerAsync(Guid id, CancellationToken ct);

    Task<bool> ExisteNitAsync(string nit, CancellationToken ct);

    Task AgregarAsync(Cliente cliente, CancellationToken ct);
}

public interface IVehiculoRepository
{
    Task<Vehiculo?> ObtenerAsync(Guid id, CancellationToken ct);

    Task<bool> ExistePlacaAsync(string placa, CancellationToken ct);

    Task AgregarAsync(Vehiculo vehiculo, CancellationToken ct);
}

public interface IConductorRepository
{
    Task<Conductor?> ObtenerAsync(Guid id, CancellationToken ct);

    Task<bool> ExisteCedulaAsync(string cedula, CancellationToken ct);

    Task AgregarAsync(Conductor conductor, CancellationToken ct);
}

public interface IOrdenRepository
{
    Task<OrdenDeCargue?> ObtenerAsync(Guid id, CancellationToken ct);

    Task AgregarAsync(OrdenDeCargue orden, CancellationToken ct);
}

public interface IViajeRepository
{
    Task<Viaje?> ObtenerAsync(Guid id, CancellationToken ct);

    Task<bool> ExisteParaOrdenAsync(Guid ordenId, CancellationToken ct);

    /// <summary>¿El vehículo está en un viaje activo distinto de <paramref name="excluirViajeId"/>?</summary>
    Task<bool> VehiculoEnViajeActivoAsync(Guid vehiculoId, Guid excluirViajeId, CancellationToken ct);

    /// <summary>¿El conductor está en un viaje activo distinto de <paramref name="excluirViajeId"/>?</summary>
    Task<bool> ConductorEnViajeActivoAsync(Guid conductorId, Guid excluirViajeId, CancellationToken ct);

    Task<string> SiguienteConsecutivoAsync(CancellationToken ct);

    Task AgregarAsync(Viaje viaje, CancellationToken ct);

    Task ActualizarAsync(Viaje viaje, CancellationToken ct);
}
