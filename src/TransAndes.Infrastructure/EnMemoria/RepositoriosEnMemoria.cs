using System.Collections.Concurrent;
using TransAndes.Application.Abstracciones;
using TransAndes.Domain.Clientes;
using TransAndes.Domain.Flota;
using TransAndes.Domain.Ordenes;
using TransAndes.Domain.Viajes;

namespace TransAndes.Infrastructure.EnMemoria;

// Repositorios en memoria para el MVP de US1 y para pruebas.
// Se reemplazarán por EF Core + Azure SQL (tarea T010) sin cambiar la capa de aplicación.

public sealed class ClienteRepositoryEnMemoria : IClienteRepository
{
    private readonly ConcurrentDictionary<Guid, Cliente> _datos = new();

    public Task<Cliente?> ObtenerAsync(Guid id, CancellationToken ct) => Task.FromResult(_datos.GetValueOrDefault(id));

    public Task<bool> ExisteNitAsync(string nit, CancellationToken ct) =>
        Task.FromResult(_datos.Values.Any(c => string.Equals(c.Nit, nit, StringComparison.OrdinalIgnoreCase)));

    public Task AgregarAsync(Cliente cliente, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cliente);
        _datos[cliente.Id] = cliente;
        return Task.CompletedTask;
    }
}

public sealed class VehiculoRepositoryEnMemoria : IVehiculoRepository
{
    private readonly ConcurrentDictionary<Guid, Vehiculo> _datos = new();

    public Task<Vehiculo?> ObtenerAsync(Guid id, CancellationToken ct) => Task.FromResult(_datos.GetValueOrDefault(id));

    public Task<bool> ExistePlacaAsync(string placa, CancellationToken ct) =>
        Task.FromResult(_datos.Values.Any(v => string.Equals(v.Placa, placa, StringComparison.Ordinal)));

    public Task AgregarAsync(Vehiculo vehiculo, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(vehiculo);
        _datos[vehiculo.Id] = vehiculo;
        return Task.CompletedTask;
    }
}

public sealed class ConductorRepositoryEnMemoria : IConductorRepository
{
    private readonly ConcurrentDictionary<Guid, Conductor> _datos = new();

    public Task<Conductor?> ObtenerAsync(Guid id, CancellationToken ct) => Task.FromResult(_datos.GetValueOrDefault(id));

    public Task<bool> ExisteCedulaAsync(string cedula, CancellationToken ct) =>
        Task.FromResult(_datos.Values.Any(c => string.Equals(c.Cedula, cedula, StringComparison.Ordinal)));

    public Task AgregarAsync(Conductor conductor, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(conductor);
        _datos[conductor.Id] = conductor;
        return Task.CompletedTask;
    }
}

public sealed class OrdenRepositoryEnMemoria : IOrdenRepository
{
    private readonly ConcurrentDictionary<Guid, OrdenDeCargue> _datos = new();

    public Task<OrdenDeCargue?> ObtenerAsync(Guid id, CancellationToken ct) => Task.FromResult(_datos.GetValueOrDefault(id));

    public Task AgregarAsync(OrdenDeCargue orden, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(orden);
        _datos[orden.Id] = orden;
        return Task.CompletedTask;
    }
}

public sealed class ViajeRepositoryEnMemoria : IViajeRepository
{
    private readonly ConcurrentDictionary<Guid, Viaje> _datos = new();
    private int _consecutivo;

    public Task<Viaje?> ObtenerAsync(Guid id, CancellationToken ct) => Task.FromResult(_datos.GetValueOrDefault(id));

    public Task<bool> ExisteParaOrdenAsync(Guid ordenId, CancellationToken ct) =>
        Task.FromResult(_datos.Values.Any(v => v.OrdenId == ordenId));

    public Task<bool> VehiculoEnViajeActivoAsync(Guid vehiculoId, Guid excluirViajeId, CancellationToken ct) =>
        Task.FromResult(_datos.Values.Any(v => v.Id != excluirViajeId && v.EstaActivo && v.VehiculoId == vehiculoId));

    public Task<bool> ConductorEnViajeActivoAsync(Guid conductorId, Guid excluirViajeId, CancellationToken ct) =>
        Task.FromResult(_datos.Values.Any(v => v.Id != excluirViajeId && v.EstaActivo && v.ConductorId == conductorId));

    public Task<string> SiguienteConsecutivoAsync(CancellationToken ct)
    {
        var numero = Interlocked.Increment(ref _consecutivo);
        return Task.FromResult($"V-{numero:D6}");
    }

    public Task AgregarAsync(Viaje viaje, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(viaje);
        _datos[viaje.Id] = viaje;
        return Task.CompletedTask;
    }

    public Task ActualizarAsync(Viaje viaje, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(viaje);
        _datos[viaje.Id] = viaje;
        return Task.CompletedTask;
    }
}
