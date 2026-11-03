using TransAndes.Application.Abstracciones;
using TransAndes.Application.Comun;
using TransAndes.Domain.Comun;
using TransAndes.Domain.Flota;

namespace TransAndes.Application.Flota;

public sealed record RegistrarVehiculoCommand(string Placa, string Tipo, int CapacidadKg, Vinculacion Vinculacion, DateOnly VenceSoat, DateOnly VenceRtm);

public sealed record RegistrarConductorCommand(string Cedula, string Nombre, string Celular, string LicenciaCategoria, DateOnly VenceLicencia);

public sealed class RegistrarVehiculoHandler(IVehiculoRepository vehiculos)
{
    public async Task<Resultado<Guid>> HandleAsync(RegistrarVehiculoCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);
        Vehiculo vehiculo;
        try
        {
            vehiculo = new Vehiculo(Guid.NewGuid(), cmd.Placa, cmd.Tipo, cmd.CapacidadKg, cmd.Vinculacion, cmd.VenceSoat, cmd.VenceRtm);
        }
        catch (DomainException ex)
        {
            return Resultado.Falla<Guid>(TipoError.Validacion, new ErrorDetalle("VEHICULO_INVALIDO", ex.Message));
        }

        if (await vehiculos.ExistePlacaAsync(vehiculo.Placa, ct).ConfigureAwait(false))
        {
            return Resultado.Falla<Guid>(TipoError.Conflicto, new ErrorDetalle("PLACA_DUPLICADA", $"Ya existe un vehículo con placa {vehiculo.Placa}."));
        }

        await vehiculos.AgregarAsync(vehiculo, ct).ConfigureAwait(false);
        return Resultado.Ok(vehiculo.Id);
    }
}

public sealed class RegistrarConductorHandler(IConductorRepository conductores)
{
    public async Task<Resultado<Guid>> HandleAsync(RegistrarConductorCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);
        Conductor conductor;
        try
        {
            conductor = new Conductor(Guid.NewGuid(), cmd.Cedula, cmd.Nombre, cmd.Celular, cmd.LicenciaCategoria, cmd.VenceLicencia);
        }
        catch (DomainException ex)
        {
            return Resultado.Falla<Guid>(TipoError.Validacion, new ErrorDetalle("CONDUCTOR_INVALIDO", ex.Message));
        }

        if (await conductores.ExisteCedulaAsync(conductor.Cedula, ct).ConfigureAwait(false))
        {
            return Resultado.Falla<Guid>(TipoError.Conflicto, new ErrorDetalle("CEDULA_DUPLICADA", "Ya existe un conductor con esa cédula."));
        }

        await conductores.AgregarAsync(conductor, ct).ConfigureAwait(false);
        return Resultado.Ok(conductor.Id);
    }
}
